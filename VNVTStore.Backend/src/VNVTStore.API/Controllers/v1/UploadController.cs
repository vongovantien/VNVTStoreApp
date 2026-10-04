using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Asp.Versioning;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Common;
using VNVTStore.Application.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("ExpensiveLimit")] // 🛡️ Rate limit file uploads
public class UploadController : ControllerBase
{
    private readonly IImageUploadService _uploadService;
    private readonly IFileValidationService _fileValidation;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        IImageUploadService uploadService,
        IFileValidationService fileValidation,
        ILogger<UploadController> logger)
    {
        _uploadService = uploadService;
        _fileValidation = fileValidation;
        _logger = logger;
    }

    /// <summary>
    /// 🛡️ SECURE FILE UPLOAD: Validates file type, size, magic number
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        try
        {
            // 🛡️ SECURITY: Validate file with magic number check
            using var stream = file.OpenReadStream();
            var validationResult = _fileValidation.ValidateImageFile(
                stream,
                file.FileName,
                file.ContentType,
                file.Length);

            if (validationResult.IsFailure)
            {
                _logger.LogWarning("File upload rejected: {Error} - File: {FileName}, ContentType: {ContentType}, Size: {Size}",
                    validationResult.Error.Message, file.FileName, file.ContentType, file.Length);
                return BadRequest(new { error = validationResult.Error.Message });
            }

            // Reset stream position after validation
            stream.Position = 0;

            // Sanitize filename
            var sanitizedFileName = _fileValidation.SanitizeFileName(file.FileName);

            // Upload to storage
            var result = await _uploadService.UploadImageAsync(stream, sanitizedFileName);

            if (result.IsFailure)
            {
                _logger.LogError("File upload failed: {Error}", result.Error.Message);
                return BadRequest(new { error = result.Error.Message });
            }

            _logger.LogInformation("File uploaded successfully: {FileName} -> {Url}", sanitizedFileName, result.Value.Url);
            return Ok(new { url = result.Value.Url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file upload");
            return StatusCode(500, new { error = "An error occurred during file upload." });
        }
    }
}
