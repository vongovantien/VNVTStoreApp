using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IImageUploadService _imageUploadService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FileService> _logger;

    public FileService(IImageUploadService imageUploadService, IApplicationDbContext context, ILogger<FileService> logger)
    {
        _imageUploadService = imageUploadService;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<string>>> SaveAndLinkImagesAsync(
        string masterCode, 
        string masterType, 
        IEnumerable<string> imageStrings, 
        string folderName = "products",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SaveAndLinkImagesAsync] Starting for MasterCode: {MasterCode}, MasterType: {MasterType}", masterCode, masterType);
        
        if (imageStrings == null || !imageStrings.Any())
        {
            _logger.LogInformation("[SaveAndLinkImagesAsync] No image strings provided.");
            return Result.Success(Enumerable.Empty<string>());
        }

        try
        {
            var base64Images = new List<(string Content, string FileName)>();
            var existingUrls = new List<string>();

            foreach (var imgStr in imageStrings)
            {
                if (IsBase64String(imgStr))
                {
                    string extension = ".png";
                    if (imgStr.StartsWith("data:image/"))
                    {
                        try
                        {
                            var mime = imgStr.Substring(5, imgStr.IndexOf(";") - 5);
                            extension = "." + mime.Split('/')[1];
                        }
                        catch { }
                    }
                    base64Images.Add((imgStr, $"{masterType.ToLower()}_{Guid.NewGuid()}{extension}"));
                }
                else
                {
                    existingUrls.Add(imgStr);
                }
            }

            _logger.LogInformation("[SaveAndLinkImagesAsync] Identified {Base64Count} base64 images and {UrlCount} existing URLs.", base64Images.Count, existingUrls.Count);

            var finalUrls = new List<string>(existingUrls);

            if (base64Images.Any())
            {
                var uploadResult = await _imageUploadService.UploadBase64ImagesAsync(base64Images, folderName);
                if (uploadResult.Value != null)
                {
                    var newUrls = uploadResult.Value.Select(f => f.Url).ToList();
                    _logger.LogInformation("[SaveAndLinkImagesAsync] Successfully uploaded base64 images. New URLs: {NewUrls}", string.Join(", ", newUrls));
                    finalUrls.AddRange(newUrls);
                }
                else if (uploadResult.IsFailure)
                {
                    _logger.LogError("[SaveAndLinkImagesAsync] Failed to upload base64 images: {Error}", uploadResult.Error);
                    return Result.Failure<IEnumerable<string>>(uploadResult.Error);
                }
            }

            // Link to Master record
            var uniqueUrls = finalUrls.Distinct().ToList();
            _logger.LogDebug("[SaveAndLinkImagesAsync] Unique URLs to link: {Urls}", string.Join(", ", uniqueUrls));

            // Expand uniqueUrls to include variants of the URL that might be stored in DB (absolute vs relative)
            var searchPaths = uniqueUrls.SelectMany(u =>
            {
                var list = new List<string> { u };
                if (Uri.TryCreate(u, UriKind.Absolute, out var uri))
                {
                    list.Add(uri.AbsolutePath);
                    list.Add(uri.AbsoluteUri);
                }
                return list;
            }).Distinct().ToList();

            _logger.LogDebug("[SaveAndLinkImagesAsync] Searching for TblFiles with paths: {Paths}", string.Join(", ", searchPaths));

            var filesToLink = await _context.TblFiles
                .Where(f => searchPaths.Contains(f.Path))
                .ToListAsync(cancellationToken);
                
            _logger.LogInformation("[SaveAndLinkImagesAsync] Found {Count} TblFiles in DB to link.", filesToLink.Count);

            foreach (var file in filesToLink)
            {
                _logger.LogDebug("[SaveAndLinkImagesAsync] Linking file {Code} (Path: {Path}) to {MasterCode}", file.Code, file.Path, masterCode);
                file.MasterCode = masterCode;
                file.MasterType = masterType;
            }

            // Identify URLs that don't have a record in DB yet (e.g. external URLs)
            var urlsInDb = filesToLink.Select(f => f.Path).ToHashSet();
            var missingUrls = uniqueUrls.Where(u => !urlsInDb.Contains(u)).ToList();

            if (missingUrls.Any())
            {
                _logger.LogInformation("[SaveAndLinkImagesAsync] Processing {Count} missing/external URLs for upload: {Urls}", missingUrls.Count, string.Join(", ", missingUrls));
                
                foreach (var url in missingUrls)
                {
                    var fileName = System.IO.Path.GetFileName(url);
                    if (string.IsNullOrEmpty(fileName) || fileName.Length < 3)
                        fileName = $"imported_{Guid.NewGuid()}";
                        
                    var uploadResult = await _imageUploadService.UploadUrlAsync(url, fileName, folderName);
                    
                    if (uploadResult.IsSuccess)
                    {
                        var newFileEntity = await _context.TblFiles.FirstOrDefaultAsync(f => f.Code == uploadResult.Value.Code, cancellationToken);
                        if (newFileEntity != null)
                        {
                            newFileEntity.MasterCode = masterCode;
                            newFileEntity.MasterType = masterType;
                            _logger.LogInformation("[SaveAndLinkImagesAsync] Uploaded and linked external URL: {Url}", uploadResult.Value.Url);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("[SaveAndLinkImagesAsync] Failed to upload/link external URL: {Url}. error: {Error}", url, uploadResult.Error);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("[SaveAndLinkImagesAsync] Successfully completed and saved changes.");

            return Result.Success((IEnumerable<string>)uniqueUrls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SaveAndLinkImagesAsync] Unexpected error during image processing for MasterCode: {MasterCode}", masterCode);
            return Result.Failure<IEnumerable<string>>(Error.Validation("An unexpected error occurred while processing images."));
        }
    }

    public async Task<Result> SyncProductImagesAsync(
        string productCode, 
        IEnumerable<string> currentImageStrings, 
        CancellationToken cancellationToken = default)
    {
        // 1. Get all currently linked files
        var existingFiles = await _context.TblFiles
            .Where(f => f.MasterCode == productCode && f.MasterType == "TblProduct")
            .ToListAsync(cancellationToken);

        var currentUrls = currentImageStrings.SelectMany(u =>
        {
             var list = new List<string> { u };
             if (Uri.TryCreate(u, UriKind.Absolute, out var uri))
                list.Add(uri.AbsolutePath);
             return list;
        }).ToHashSet(); 

        // 2. Identify files to unlink AND DELETE
        var toUnlink = existingFiles.Where(f => !currentUrls.Contains(f.Path)).ToList();
        
        if (toUnlink.Any())
        {
             // Call DeleteImagesAsync to remove from Cloudinary
             var urlsToDelete = toUnlink.Select(f => f.Path).ToList();
             await _imageUploadService.DeleteImagesAsync(urlsToDelete);

             // Remove from Database (Soft Delete or Hard Delete? Logic implies DeleteLinkedFilesAsync loops active=false. 
             // But usually for Sync we might want to just Unlink OR Delete. 
             // If we delete from Cloudinary, we MUST delete/deactivate from DB to avoid broken links if they were reused (less likely for specific Product Image).
             // Assuming TblFile is 1-1 with Product linkage usually.)
             
             _context.TblFiles.RemoveRange(toUnlink); // Hard remove from DB to keep it clean, as we deleted from Cloud
        }

        // 3. Process new images
        await SaveAndLinkImagesAsync(productCode, "TblProduct", currentImageStrings, "products", cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteLinkedFilesAsync(string masterCode, string masterType, CancellationToken cancellationToken = default)
    {
        var linkedFiles = await _context.TblFiles
            .Where(f => f.MasterCode == masterCode && f.MasterType == masterType)
            .ToListAsync(cancellationToken);

        if (!linkedFiles.Any())
            return Result.Success();

        var pathsToDelete = linkedFiles.Select(f => f.Path).Where(p => !string.IsNullOrEmpty(p)).ToList();

        if (pathsToDelete.Any())
        {
            await _imageUploadService.DeleteImagesAsync(pathsToDelete);
        }

        foreach (var file in linkedFiles)
        {
            file.IsActive = false;
            file.MasterCode = null;
            file.MasterType = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private bool IsBase64String(string s) => s.StartsWith("data:image");
}
