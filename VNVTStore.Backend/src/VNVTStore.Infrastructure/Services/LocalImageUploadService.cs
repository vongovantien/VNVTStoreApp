using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Infrastructure.Services;

public class LocalImageUploadService : IImageUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly IApplicationDbContext _context;

    public LocalImageUploadService(IWebHostEnvironment env, IApplicationDbContext context)
    {
        _env = env;
        _context = context;
    }

    public async Task<Result<FileDto>> UploadImageAsync(Stream imageStream, string fileName, string folder = "products")
    {
        try
        {
            // Ensure wwwroot exists (in case it doesn't)
            if (string.IsNullOrWhiteSpace(_env.WebRootPath))
            {
                _env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }
            
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads", folder);
            
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            long fileSize = imageStream.Length;
            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            // Return relative URL
            // Ensure forward slashes for URL
            var url = $"/uploads/{folder}/{uniqueFileName}";

            // Map to FileDto
            var fileDto = new FileDto
            {
                Code = Guid.NewGuid().ToString(), // Temp code if not saved to DB yet, or use ID logic
                FileName = uniqueFileName,
                OriginalName = fileName,
                Extension = extension,
                MimeType = "image/" + extension.TrimStart('.').ToLower(),
                Size = fileSize,
                Path = filePath,
                Url = url
            };

            // Save to Database
            var fileEntity = VNVTStore.Domain.Entities.TblFile.Create(
                uniqueFileName,
                fileName,
                extension,
                fileDto.MimeType,
                fileSize,
                url
            );
            
            // Manual Code Generation to avoid DB sequence issues
            fileEntity.Code = $"FIL{DateTime.Now.Ticks}"; 

            _context.TblFiles.Add(fileEntity);
            await _context.SaveChangesAsync(CancellationToken.None);
            
            return Result.Success(fileDto);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? "";
            Console.WriteLine($"[Error] Upload failed: {ex} {innerMessage}");
            return Result.Failure<FileDto>(Error.Validation($"Failed to upload image: {ex.Message} {innerMessage}"));
        }
    }

    public async Task<Result<IEnumerable<FileDto>>> UploadImagesAsync(IEnumerable<(Stream Stream, string FileName)> images, string folder = "products")
    {
        var files = new List<FileDto>();
        foreach (var image in images)
        {
            var result = await UploadImageAsync(image.Stream, image.FileName, folder);
            if (result.IsFailure)
            {
                return Result.Failure<IEnumerable<FileDto>>(result.Error!);
            }
            files.Add(result.Value!);
        }
        return Result.Success((IEnumerable<FileDto>)files);
    }

    public Task<Result> DeleteImageAsync(string imageUrl)
    {
        try
        {
             if (string.IsNullOrWhiteSpace(_env.WebRootPath))
            {
                _env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            // Convert URL to file path
            // Remove starting / if present
            var relativePath = imageUrl.TrimStart('/');
            var filePath = Path.Combine(_env.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure(Error.Validation("Delete", $"Failed to delete image: {ex.Message}")));
        }
    }

    public async Task<Result<FileDto>> UploadBase64Async(string base64Content, string fileName, string folder = "products")
    {
        try
        {
            // 1. Strip Data URI scheme if present (e.g., "data:image/png;base64,")
            var data = base64Content;
            if (data.Contains(","))
            {
                data = data.Split(',')[1];
            }

            // 2. Convert Base64 string to byte[]
            byte[] bytes;
            try 
            {
                bytes = Convert.FromBase64String(data);
            }
            catch (FormatException)
            {
                return Result.Failure<FileDto>(Error.Validation("Invalid Base64 string format"));
            }

            // 3. Create stream and reuse UploadImageAsync
            using (var stream = new MemoryStream(bytes))
            {
                return await UploadImageAsync(stream, fileName, folder);
            }
        }
        catch (Exception ex)
        {
             Console.WriteLine($"[Error] Base64 Upload failed: {ex}");
             return Result.Failure<FileDto>(Error.Validation($"Failed to upload base64 image: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<FileDto>>> UploadBase64ImagesAsync(IEnumerable<(string Base64Content, string FileName)> images, string folder = "products")
    {
         var files = new List<FileDto>();
        foreach (var image in images)
        {
            var result = await UploadBase64Async(image.Base64Content, image.FileName, folder);
            if (result.IsFailure)
            {
                return Result.Failure<IEnumerable<FileDto>>(result.Error!);
            }
            files.Add(result.Value!);
        }
        return Result.Success((IEnumerable<FileDto>)files);
    }

    public async Task<Result> DeleteImagesAsync(IEnumerable<string> imageUrls)
    {
        foreach (var url in imageUrls)
        {
            var result = await DeleteImageAsync(url);
            if (result.IsFailure)
            {
                return result;
            }
        }
        return Result.Success();
    }
}
