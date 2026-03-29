using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.SystemSecret.Commands;
using VNVTStore.Application.SystemSecret.Queries;
using VNVTStore.Domain.Enums;

namespace VNVTStore.API.Controllers.v1;

[Authorize(Roles = nameof(UserRole.Admin))]
public class SystemSecretsController : BaseApiController
{
    public SystemSecretsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Lấy danh sách cấu hình bí mật
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemSecretDto>>), Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemSecrets()
    {
        var result = await Mediator.Send(new GetSystemSecretsQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Thêm hoặc cập nhật cấu hình bí mật
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<bool>), Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSystemSecret([FromBody] UpdateSecretRequest request)
    {
        var command = new UpdateSystemSecretCommand(request.Key, request.Value, request.Description);
        var result = await Mediator.Send(command);
        return HandleResult(result, "Secret updated successfully.");
    }

    /// <summary>
    /// Xóa cấu hình bí mật
    /// </summary>
    [HttpDelete("{key}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteSystemSecret(string key)
    {
        var command = new DeleteSystemSecretCommand(key);
        var result = await Mediator.Send(command);
        return HandleResult(result, "Secret deleted successfully.");
    }

    /// <summary>
    /// Xuáº¥t Excel danh sÃ¡ch bÃ­ máº­t
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var result = await Mediator.Send(new ExportSystemSecretsQuery());
        if (!result.IsSuccess || result.Value == null) return HandleResult(result);
        return File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SystemSecrets_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    /// <summary>
    /// Nháº­p Excel danh sÃ¡ch bÃ­ máº­t
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");
        using var stream = file.OpenReadStream();
        var result = await Mediator.Send(new ImportSystemSecretsCommand(stream));
        return HandleResult(result);
    }
}

public record UpdateSecretRequest(string Key, string Value, string? Description);
