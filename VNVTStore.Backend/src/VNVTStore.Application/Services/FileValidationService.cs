using System.Security.Cryptography;
using VNVTStore.Application.Common;
using VNVTStore.Domain.Common;

namespace VNVTStore.Application.Services;

/// <summary>
/// 🛡️ FILE UPLOAD SECURITY: Validates file uploads to prevent malicious content
/// - Magic number verification (real file type detection)
/// - Extension whitelist
/// - MIME type validation
/// - File size limits
/// - Filename sanitization
/// </summary>
public interface IFileValidationService
{
    Result ValidateImageFile(Stream fileStream, string fileName, string contentType, long fileSize);
    string SanitizeFileName(string fileName);
}

public class FileValidationService : IFileValidationService
{
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB general

    // Allowed image extensions
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    // Allowed MIME types
    private static readonly HashSet<string> AllowedImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    // Magic numbers (file signatures) for image validation
    private static readonly Dictionary<string, byte[][]> ImageMagicNumbers = new()
    {
        { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".gif", new[] { 
            new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, // GIF87a
            new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }  // GIF89a
        } },
        { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } } // RIFF (WebP container)
    };

    public Result ValidateImageFile(Stream fileStream, string fileName, string contentType, long fileSize)
    {
        // 1. Validate file size
        if (fileSize <= 0)
            return Result.Failure(Error.Validation("File is empty"));

        if (fileSize > MaxImageSizeBytes)
            return Result.Failure(Error.Validation($"File size exceeds maximum limit of {MaxImageSizeBytes / 1024 / 1024}MB"));

        // 2. Validate extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
            return Result.Failure(Error.Validation($"File extension '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedImageExtensions)}"));

        // 3. Validate MIME type
        if (string.IsNullOrEmpty(contentType) || !AllowedImageMimeTypes.Contains(contentType.ToLowerInvariant()))
            return Result.Failure(Error.Validation($"MIME type '{contentType}' is not allowed. Allowed: {string.Join(", ", AllowedImageMimeTypes)}"));

        // 4. Validate magic number (file signature)
        var magicNumberResult = ValidateMagicNumber(fileStream, extension);
        if (magicNumberResult.IsFailure)
            return magicNumberResult;

        // 5. Validate filename
        var sanitizedFileName = SanitizeFileName(fileName);
        if (string.IsNullOrEmpty(sanitizedFileName))
            return Result.Failure(Error.Validation("Invalid filename"));

        return Result.Success();
    }

    private Result ValidateMagicNumber(Stream fileStream, string extension)
    {
        try
        {
            if (!ImageMagicNumbers.TryGetValue(extension, out var expectedSignatures))
                return Result.Failure(Error.Validation($"No magic number signature found for extension '{extension}'"));

            // Read first bytes from stream
            var buffer = new byte[8]; // Max signature length
            var originalPosition = fileStream.Position;
            fileStream.Position = 0;
            var bytesRead = fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Position = originalPosition; // Reset position

            if (bytesRead == 0)
                return Result.Failure(Error.Validation("Unable to read file signature"));

            // Check if file starts with any of the expected signatures
            var matchesSignature = expectedSignatures.Any(signature =>
            {
                if (bytesRead < signature.Length)
                    return false;

                for (int i = 0; i < signature.Length; i++)
                {
                    if (buffer[i] != signature[i])
                        return false;
                }
                return true;
            });

            if (!matchesSignature)
                return Result.Failure(Error.Validation($"File signature does not match expected type for '{extension}'. Possible file type spoofing detected."));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Validation($"Error validating file signature: {ex.Message}"));
        }
    }

    /// <summary>
    /// Sanitizes filename to prevent directory traversal and injection attacks
    /// </summary>
    public string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        // Remove path information
        fileName = Path.GetFileName(fileName);

        // Remove invalid filename characters
        var invalidChars = Path.GetInvalidFileNameChars();
        fileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Remove dangerous patterns
        fileName = fileName
            .Replace("..", "_")
            .Replace("./", "_")
            .Replace("../", "_")
            .Replace("\\", "_")
            .Replace("/", "_");

        // Limit length
        var extension = Path.GetExtension(fileName);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        
        if (nameWithoutExt.Length > 100)
            nameWithoutExt = nameWithoutExt.Substring(0, 100);

        // Generate safe filename with timestamp to avoid collisions
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var hash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(fileName))).Substring(0, 8);
        
        return $"{nameWithoutExt}_{timestamp}_{hash}{extension}".ToLowerInvariant();
    }
}
