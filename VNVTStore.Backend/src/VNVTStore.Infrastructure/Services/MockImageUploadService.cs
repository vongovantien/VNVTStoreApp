using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class MockImageUploadService : IImageUploadService
{
    public Task<Result<FileDto>> UploadImageAsync(Stream imageStream, string fileName, string folder = "products")
    {
        var fileDto = new FileDto
        {
            Code = "MOCK_FILE_" + Guid.NewGuid().ToString("N"),
            FileName = fileName,
            OriginalName = fileName,
            Extension = Path.GetExtension(fileName),
            MimeType = "image/png",
            Size = 100,
            Path = "https://example.com/mock-image.png",
            Url = "https://example.com/mock-image.png"
        };
        return Task.FromResult(Result.Success(fileDto));
    }

    public Task<Result<IEnumerable<FileDto>>> UploadImagesAsync(IEnumerable<(Stream Stream, string FileName)> images, string folder = "products")
    {
        var files = images.Select(i => new FileDto
        {
            Code = "MOCK_FILE_" + Guid.NewGuid().ToString("N"),
            FileName = i.FileName,
            OriginalName = i.FileName,
            Extension = Path.GetExtension(i.FileName),
            MimeType = "image/png",
            Size = 100,
            Path = "https://example.com/mock-image.png",
            Url = "https://example.com/mock-image.png"
        }).ToList();
        return Task.FromResult(Result.Success((IEnumerable<FileDto>)files));
    }

    public Task<Result> DeleteImageAsync(string imageUrl) => Task.FromResult(Result.Success());
    public Task<Result> DeleteImagesAsync(IEnumerable<string> imageUrls) => Task.FromResult(Result.Success());

    public Task<Result<FileDto>> UploadBase64Async(string base64Content, string fileName, string folder = "products")
    {
         return UploadImageAsync(Stream.Null, fileName, folder);
    }

    public Task<Result<IEnumerable<FileDto>>> UploadBase64ImagesAsync(IEnumerable<(string Base64Content, string FileName)> images, string folder = "products")
    {
         return UploadImagesAsync(images.Select(i => (Stream.Null, i.FileName)), folder);
    }

    public Task<Result<FileDto>> UploadUrlAsync(string url, string fileName, string folder = "products")
    {
         return UploadImageAsync(Stream.Null, fileName, folder);
    }
}
