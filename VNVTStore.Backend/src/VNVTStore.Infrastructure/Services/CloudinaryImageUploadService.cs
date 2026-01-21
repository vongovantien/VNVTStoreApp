using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Infrastructure.Services;

public class CloudinaryImageUploadService : IImageUploadService
{
    private readonly Cloudinary _cloudinary;
    private readonly IApplicationDbContext _context;

    public CloudinaryImageUploadService(IConfiguration configuration, IApplicationDbContext context)
    {
        _context = context;
        // Parse CLOUDINARY_URL or individual keys
        var cloudinaryUrl = configuration["CLOUDINARY_URL"];
        if (!string.IsNullOrEmpty(cloudinaryUrl))
        {
            _cloudinary = new Cloudinary(cloudinaryUrl);
        }
        else
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }
        _cloudinary.Api.Secure = true;
    }

    public async Task<Result<FileDto>> UploadImageAsync(Stream imageStream, string fileName, string folder = "products")
    {
        try
        {
            if (imageStream.Position > 0)
                imageStream.Position = 0;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = folder,
                PublicId = Path.GetFileNameWithoutExtension(fileName) // Optional: Use filename as public_id (or let Cloudinary generate)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return Result.Failure<FileDto>(VNVTStore.Application.Common.Error.Validation($"Cloudinary VNVTStore.Application.Common.Error: {uploadResult.Error.Message}"));
            }

            // Map to FileDto
            var fileDto = new FileDto
            {
                Code = Guid.NewGuid().ToString(), // Temporary, or mapped from Entity later
                FileName = fileName,
                OriginalName = fileName,
                Extension = Path.GetExtension(fileName),
                MimeType = "image/" + uploadResult.Format,
                Size = uploadResult.Bytes,
                Path = uploadResult.SecureUrl.AbsoluteUri, // Store Full URL as Path for Cloudinary
                Url = uploadResult.SecureUrl.AbsoluteUri
            };

            // Save to Database
            // Note: For Cloudinary, Path and Url are usually the same (the remote URL)
            var fileEntity = TblFile.Create(
                fileName,
                fileName, // Original Name
                Path.GetExtension(fileName),
                fileDto.MimeType,
                fileDto.Size,
                fileDto.Url
            );
            
            // Manual Code Gen
            fileEntity.Code = $"FIL{DateTime.Now.Ticks}";

            _context.TblFiles.Add(fileEntity);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Result.Success(fileDto);
        }
        catch (Exception ex)
        {
             return Result.Failure<FileDto>(VNVTStore.Application.Common.Error.Validation($"Upload failed: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<FileDto>>> UploadImagesAsync(IEnumerable<(Stream Stream, string FileName)> images, string folder = "products")
    {
        var uploadTasks = images.Select(image => UploadImageAsync(image.Stream, image.FileName, folder)).ToList();
        var results = await Task.WhenAll(uploadTasks);

        var files = new List<FileDto>();
        foreach (var result in results)
        {
            if (result.IsFailure) return Result.Failure<IEnumerable<FileDto>>(result.Error!);
            files.Add(result.Value!);
        }
        
        return Result.Success((IEnumerable<FileDto>)files);
    }

    public async Task<Result> DeleteImageAsync(string imageUrl)
    {
        return await DeleteImagesAsync(new[] { imageUrl });
    }

    public async Task<Result> DeleteImagesAsync(IEnumerable<string> imageUrls)
    {
        try
        {
            if (imageUrls == null || !imageUrls.Any()) return Result.Success();

            var publicIds = new List<string>();
            foreach (var url in imageUrls)
            {
                 if (string.IsNullOrEmpty(url)) continue;
                 
                 // Extract Public ID logic (simplified)
                 // Assumes standard Cloudinary URL structure
                 try 
                 {
                     var uri = new Uri(url);
                     var path = uri.AbsolutePath;
                     var uploadIndex = path.IndexOf("/upload/");
                     if (uploadIndex != -1)
                     {
                        var afterUpload = path.Substring(uploadIndex + 8);
                        var parts = afterUpload.Split('/');
                        
                        // Skip version if present
                        var startPartIndex = 0;
                        if (parts.Length > 0 && parts[0].StartsWith("v") && parts[0].Skip(1).All(char.IsDigit))
                        {
                            startPartIndex = 1;
                        }

                        var publicIdParts = parts.Skip(startPartIndex).ToArray();
                        var publicIdWithExt = string.Join("/", publicIdParts);
                        var publicId = publicIdWithExt.Substring(0, publicIdWithExt.LastIndexOf('.'));
                        publicIds.Add(publicId);
                     }
                 }
                 catch {}
            }

            if (publicIds.Any())
            {
                // Cloudinary allows deleting multiple resources by Public IDs
                // Note: Max 100 per call usually, might need chunking if list is huge
                // Assuming reasonable batch size for now or implementing simple chunking
                
                const int batchSize = 100;
                var chunks = publicIds.Chunk(batchSize);

                foreach (var chunk in chunks)
                {
                    var delParams = new DelResParams
                    {
                        PublicIds = chunk.ToList()
                    };
                    await _cloudinary.DeleteResourcesAsync(delParams);
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(VNVTStore.Application.Common.Error.Validation($"Bulk delete failed: {ex.Message}"));
        }
    }

    public async Task<Result<FileDto>> UploadBase64Async(string base64Content, string fileName, string folder = "products")
    {
        try
        {
             var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(base64Content), 
                Folder = folder,
                PublicId = Path.GetFileNameWithoutExtension(fileName) 
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return Result.Failure<FileDto>(VNVTStore.Application.Common.Error.Validation($"Cloudinary upload failed: {uploadResult.Error.Message}"));
            }

            var fileDto = new FileDto
            {
                Code = Guid.NewGuid().ToString(),
                FileName = fileName,
                OriginalName = fileName,
                Extension = Path.GetExtension(fileName),
                MimeType = "image/" + uploadResult.Format,
                Size = uploadResult.Bytes,
                Path = uploadResult.SecureUrl.AbsoluteUri,
                Url = uploadResult.SecureUrl.AbsoluteUri
            };

             var fileEntity = TblFile.Create(
                fileName,
                fileName,
                Path.GetExtension(fileName),
                fileDto.MimeType,
                fileDto.Size,
                fileDto.Url
            );
            fileEntity.Code = $"FIL{DateTime.Now.Ticks}";

            _context.TblFiles.Add(fileEntity);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Result.Success(fileDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<FileDto>(VNVTStore.Application.Common.Error.Validation($"Base64 Upload failed: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<FileDto>>> UploadBase64ImagesAsync(IEnumerable<(string Base64Content, string FileName)> images, string folder = "products")
    {
        // 1. Sequential Upload to Cloudinary (SAFE)
        var uploadedFiles = new List<FileDto>();
        var errors = new List<string>();

        foreach (var image in images)
        {
            try 
            {
                 var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.Base64Content), 
                    Folder = folder,
                    PublicId = Path.GetFileNameWithoutExtension(image.FileName) 
                };

                // Sequential await to avoid overwhelming network or hitting rate limits aggressively
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    errors.Add($"Failed to upload {image.FileName}: {uploadResult.Error.Message}");
                    continue;
                }

                var fileDto = new FileDto
                {
                    Code = Guid.NewGuid().ToString(),
                    FileName = image.FileName,
                    OriginalName = image.FileName,
                    Extension = Path.GetExtension(image.FileName),
                    MimeType = "image/" + uploadResult.Format,
                    Size = uploadResult.Bytes,
                    Path = uploadResult.SecureUrl.AbsoluteUri,
                    Url = uploadResult.SecureUrl.AbsoluteUri
                };
                uploadedFiles.Add(fileDto);
            }
            catch (Exception ex)
            {
                errors.Add($"Exception uploading {image.FileName}: {ex.Message}");
            }
        }

        if (!uploadedFiles.Any() && errors.Any())
        {
            return Result.Failure<IEnumerable<FileDto>>(VNVTStore.Application.Common.Error.Validation($"All uploads failed. Errors: {string.Join("; ", errors)}"));
        }

        // 2. Batch Insert to Database (FAST & SAFE)
        try
        {
            var entities = new List<TblFile>();
            foreach (var dto in uploadedFiles)
            {
                var entity = TblFile.Create(
                    dto.FileName,
                    dto.OriginalName,
                    dto.Extension,
                    dto.MimeType,
                    dto.Size,
                    dto.Url
                );
                // Ensure unique codes if generating manually based on ticks
                // Adding a small delay or random suffix might be needed if tight loop
                // Or just use Guid for Code if schema allows, but schema seems to use FIL+Ticks
                // We'll trust new Ticks will change, or use a counter.
                
                // Better: Use a counter offset for ticks to ensure uniqueness in this batch
                var tickOffset = uploadedFiles.IndexOf(dto);
                entity.Code = $"FIL{DateTime.Now.Ticks + tickOffset}";
                
                entities.Add(entity);
            }

            if (entities.Any())
            {
                await _context.TblFiles.AddRangeAsync(entities);
                await _context.SaveChangesAsync(CancellationToken.None);
            }

            return Result.Success((IEnumerable<FileDto>)uploadedFiles);
        }
        catch (Exception ex)
        {
             // If DB save fails, we ideally should rollback Cloudinary uploads (compensation transaction)
             // For now, return failure.
             return Result.Failure<IEnumerable<FileDto>>(VNVTStore.Application.Common.Error.Validation($"Database save failed: {ex.Message}"));
        }
    }
}
