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

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IImageUploadService _uploadService;

    public UploadController(IImageUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var result = await _uploadService.UploadImageAsync(stream, file.FileName);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { url = result.Value.Url });
    }
}
