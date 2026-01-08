using VNVTStore.Application.Common;

namespace VNVTStore.Application.Interfaces;

/// <summary>
/// Image upload service interface
/// </summary>
public interface IImageUploadService
{
    Task<Result<string>> UploadImageAsync(Stream imageStream, string fileName, string folder = "products");
    Task<Result<IEnumerable<string>>> UploadImagesAsync(IEnumerable<(Stream Stream, string FileName)> images, string folder = "products");
    Task<Result> DeleteImageAsync(string imageUrl);
}
