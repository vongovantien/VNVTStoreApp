using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;

namespace VNVTStore.API.Controllers;

/// <summary>
/// Base API Controller - tất cả controllers kế thừa từ đây
/// Cung cấp các helper methods chung để xử lý Result pattern
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected BaseApiController(IMediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>
    /// Xử lý Result và trả về IActionResult phù hợp
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result, string successMessage = "Success")
    {
        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        return Ok(ApiResponse<T>.Ok(result.Value!, successMessage));
    }

    /// <summary>
    /// Xử lý Result cho Create operations (trả về 201 Created)
    /// </summary>
    protected IActionResult HandleCreated<T>(Result<T> result, string actionName, object routeValues, string successMessage = "Created successfully")
    {
        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        return CreatedAtAction(
            actionName,
            routeValues,
            ApiResponse<T>.Created(result.Value!, successMessage));
    }

    /// <summary>
    /// Xử lý Result cho Delete operations (trả về 204 NoContent)
    /// </summary>
    protected IActionResult HandleDelete(Result result)
    {
        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        return NoContent();
    }

    /// <summary>
    /// Xử lý Error và trả về response phù hợp dựa trên error code
    /// </summary>
    protected IActionResult HandleError(Error error)
    {
        return error.Code switch
        {
            "NotFound" => NotFound(ApiResponse<string>.NotFound(error.Message)),
            "Validation" => BadRequest(ApiResponse<string>.Fail(error.Message)),
            "Conflict" => Conflict(ApiResponse<string>.Fail(error.Message, 409)),
            "Unauthorized" => Unauthorized(ApiResponse<string>.Unauthorized(error.Message)),
            "Forbidden" => StatusCode(403, ApiResponse<string>.Forbidden(error.Message)),
            _ => BadRequest(ApiResponse<string>.Fail(error.Message))
        };
    }

    /// <summary>
    /// Lấy User ID từ JWT claims (nếu authenticated)
    /// </summary>
    protected int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("id") ?? User.FindFirst("sub");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// Kiểm tra user có role Admin không
    /// </summary>
    protected bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }
}
