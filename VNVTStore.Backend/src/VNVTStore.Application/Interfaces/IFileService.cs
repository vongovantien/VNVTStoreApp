using VNVTStore.Application.Common;

namespace VNVTStore.Application.Interfaces;

/// <summary>
/// Service để xử lý file upload và linking
/// Đặt trong Application vì sử dụng Result pattern từ Application layer
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Processes a list of image strings (URLs or Base64), uploads new ones, 
    /// and links them to a master record.
    /// </summary>
    Task<Result<IEnumerable<string>>> SaveAndLinkImagesAsync(
        string masterCode, 
        string masterType, 
        IEnumerable<string> imageStrings, 
        string folderName = "products",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes files that are no longer linked to a record.
    /// </summary>
    Task<Result> SyncProductImagesAsync(
        string productCode, 
        IEnumerable<string> currentImageStrings, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all files linked to a master record.
    /// </summary>
    Task<Result> DeleteLinkedFilesAsync(string masterCode, string masterType, CancellationToken cancellationToken = default);
}
