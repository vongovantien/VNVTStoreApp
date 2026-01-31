using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IImageUploadService _imageUploadService;
    private readonly IApplicationDbContext _context;

    public FileService(IImageUploadService imageUploadService, IApplicationDbContext context)
    {
        _imageUploadService = imageUploadService;
        _context = context;
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
        var relativeUrls = uniqueUrls.Select(u =>
        {
            if (Uri.TryCreate(u, UriKind.Absolute, out var uri))
                return uri.AbsolutePath;
            return u;
        }).ToList();

        var filesToLink = await _context.TblFiles
            .Where(f => relativeUrls.Contains(f.Path))
            .ToListAsync(cancellationToken);

        foreach (var file in filesToLink)
        {
            file.MasterCode = masterCode;
            file.MasterType = masterType;
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

        var currentUrls = currentImageStrings.Select(u =>
        {
             if (Uri.TryCreate(u, UriKind.Absolute, out var uri))
                return uri.AbsolutePath;
            return u;
        }).ToList();

        // 2. Identify files to unlink (removed from request)
        var toUnlink = existingFiles.Where(f => !currentUrls.Contains(f.Path)).ToList();
        
        foreach(var file in toUnlink)
        {
            file.MasterCode = null;
            file.MasterType = null;
        }

        // 3. Process new images (SaveAndLink will handle base64 and link existing ones)
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
