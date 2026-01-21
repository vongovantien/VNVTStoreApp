using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Interfaces;

/// <summary>
/// Image upload service interface
/// </summary>
public interface IImageUploadService
{
    Task<Result<FileDto>> UploadImageAsync(Stream imageStream, string fileName, string folder = "products");
    Task<Result<IEnumerable<FileDto>>> UploadImagesAsync(IEnumerable<(Stream Stream, string FileName)> images, string folder = "products");
    
    Task<Result<FileDto>> UploadBase64Async(string base64Content, string fileName, string folder = "products");
    Task<Result<IEnumerable<FileDto>>> UploadBase64ImagesAsync(IEnumerable<(string Base64Content, string FileName)> images, string folder = "products");

    Task<Result> DeleteImageAsync(string imageUrl);
    Task<Result> DeleteImagesAsync(IEnumerable<string> imageUrls);
}
