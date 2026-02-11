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
        if (imageStrings == null || !imageStrings.Any())
            return Result.Success(Enumerable.Empty<string>());

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

        var finalUrls = new List<string>(existingUrls);

        if (base64Images.Any())
        {
            var uploadResult = await _imageUploadService.UploadBase64ImagesAsync(base64Images, folderName);
            if (uploadResult.IsFailure)
                return Result.Failure<IEnumerable<string>>(uploadResult.Error);

            finalUrls.AddRange(uploadResult.Value.Select(f => f.Url));
        }

        // Link to Master record
        var uniqueUrls = finalUrls.Distinct().ToList();
        var relativeUrls = uniqueUrls.SelectMany(u =>
        {
            var list = new List<string> { u };
            if (Uri.TryCreate(u, UriKind.Absolute, out var uri))
                list.Add(uri.AbsolutePath);
            return list;
        }).ToList();

        var filesToLink = await _context.TblFiles
            .Where(f => relativeUrls.Contains(f.Path))
            .ToListAsync(cancellationToken);
            
        var uniqueUrlsFoundInDb = filesToLink.Select(f => f.Path).ToHashSet();

        foreach (var file in filesToLink)
        {
            file.MasterCode = masterCode;
            file.MasterType = masterType;
        }

        // Identify "External" URLs that might have been saved to DB but not uploaded yet (legacy data or partial state)
        var externalUrlsInDb = filesToLink
            .Where(f => f.Path.StartsWith("http", StringComparison.OrdinalIgnoreCase) 
                        && !f.Path.Contains("res.cloudinary.com") 
                        && !f.Path.Contains("/uploads/"))
            .Select(f => f.Path)
            .ToList();

        // Create TblFile for URLs that do not exist in DB yet
        var missingUrls = uniqueUrls.Except(uniqueUrlsFoundInDb).ToList();
        
        // Also process "External" URLs found in DB to migrate them
        var urlsToProcess = missingUrls.Union(externalUrlsInDb).Distinct().ToList();

        if (urlsToProcess.Any())
        {
            _logger.LogInformation("[SaveAndLinkImagesAsync] Processing {Count} URLs to upload: {Urls}", urlsToProcess.Count, string.Join(", ", urlsToProcess));
            
            foreach (var url in urlsToProcess)
            {
                _logger.LogDebug("[SaveAndLinkImagesAsync] Attempting to upload URL: {Url}", url);
                
                // Upload External URL to Cloudinary
                var fileName = System.IO.Path.GetFileName(url);
                if (string.IsNullOrEmpty(fileName) || fileName.Length < 3)
                    fileName = $"imported_{Guid.NewGuid()}";
                
                _logger.LogDebug("[SaveAndLinkImagesAsync] Using filename: {FileName}, folder: {FolderName}", fileName, folderName);
                    
                var uploadResult = await _imageUploadService.UploadUrlAsync(url, fileName, folderName);
                
                if (uploadResult.IsSuccess)
                {
                    var newFileCode = uploadResult.Value.Code;
                    _logger.LogInformation("[SaveAndLinkImagesAsync] Upload successful! Code: {Code}, URL: {Url}", newFileCode, uploadResult.Value.Url);
                    
                    // Case 1: Newly created file (from missingUrls)
                    var fileEntity = await _context.TblFiles.FirstOrDefaultAsync(f => f.Code == newFileCode, cancellationToken);
                    _logger.LogDebug("[SaveAndLinkImagesAsync] Found entity by code {Code}: {Found}", newFileCode, fileEntity != null);
                    
                    if (fileEntity != null)
                    {
                        fileEntity.MasterCode = masterCode;
                        fileEntity.MasterType = masterType;
                        _logger.LogInformation("[SaveAndLinkImagesAsync] Linked file {Code} to {MasterType}:{MasterCode}", newFileCode, masterType, masterCode);
                    }

                    // Case 2: Existing file in DB that was replaced (from externalUrlsInDb)
                    // If we uploaded an existing external URL, UploadUrlAsync created a NEW TblFile.
                    // We should probably DELETE the old one to avoid duplicates?
                    // The old one matches `url`.
                    if (externalUrlsInDb.Contains(url))
                    {
                        var oldEntities = await _context.TblFiles
                            .Where(f => f.Path == url && f.MasterCode == masterCode)
                            .ToListAsync(cancellationToken);
                        _logger.LogDebug("[SaveAndLinkImagesAsync] Removing {Count} old external URL entries", oldEntities.Count);
                        _context.TblFiles.RemoveRange(oldEntities);
                    }
                    
                    uniqueUrls.Remove(url);
                    uniqueUrls.Add(uploadResult.Value.Url);
                }
                else
                {
                    _logger.LogWarning("[SaveAndLinkImagesAsync] Error: Upload FAILED for {Url}. Keeping original URL in list", url);
                }
            }
        }
        else
        {
            _logger.LogDebug("[SaveAndLinkImagesAsync] No URLs to process. uniqueUrls: {UniqueUrls}, uniqueUrlsFoundInDb: {FoundUrls}", string.Join(", ", uniqueUrls), string.Join(", ", uniqueUrlsFoundInDb));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success((IEnumerable<string>)uniqueUrls);
    }

    public async Task<Result> SyncProductImagesAsync(
        string productCode, 
        IEnumerable<string> currentImageStrings, 
        CancellationToken cancellationToken = default)
    {
        // 1. Get all currently linked files
        var existingFiles = await _context.TblFiles
            .Where(f => f.MasterCode == productCode && f.MasterType == "Product")
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
        await SaveAndLinkImagesAsync(productCode, "Product", currentImageStrings, "products", cancellationToken);

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
