using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class LocalImageUploadService : IImageUploadService
{
    private readonly IWebHostEnvironment _env;

    public LocalImageUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<Result<string>> UploadImageAsync(Stream imageStream, string fileName, string folder = "products")
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

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            // Return relative URL
            // Ensure forward slashes for URL
            var url = $"/uploads/{folder}/{uniqueFileName}";
            
            return Result.Success(url);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Validation("Upload", $"Failed to upload image: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<string>>> UploadImagesAsync(IEnumerable<(Stream Stream, string FileName)> images, string folder = "products")
    {
        var urls = new List<string>();
        foreach (var image in images)
        {
            var result = await UploadImageAsync(image.Stream, image.FileName, folder);
            if (result.IsFailure)
            {
                return Result.Failure<IEnumerable<string>>(result.Error!);
            }
            urls.Add(result.Value!);
        }
        return Result.Success((IEnumerable<string>)urls);
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
}
