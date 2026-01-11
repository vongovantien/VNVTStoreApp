using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;

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
    protected IActionResult HandleResult<T>(Result<T> result, string? successMessage = null)
    {
        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        successMessage ??= MessageConstants.Get(MessageConstants.Success);
        return Ok(ApiResponse<T>.Ok(result.Value!, successMessage));
    }

    /// <summary>
    /// Xử lý Result cho Create operations (trả về 201 Created)
    /// </summary>
    protected IActionResult HandleCreated<T>(Result<T> result, string actionName, object routeValues, string? successMessage = null)
    {
        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        successMessage ??= MessageConstants.Get(MessageConstants.Created);
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

        return Ok(ApiResponse<object>.Ok(null, MessageConstants.Get(MessageConstants.Deleted)));
    }

    /// <summary>
    /// Xử lý Error và trả về response phù hợp dựa trên error code
    /// </summary>
    protected IActionResult HandleError(Error error)
    {
        // Translate message key to human-readable message
        var message = error.Message != null ? MessageConstants.Get(error.Message) : null;
        
        return error.Code switch
        {
            "NotFound" => NotFound(ApiResponse<string>.NotFound(message ?? MessageConstants.Get(MessageConstants.NotFound))),
            "Validation" => BadRequest(ApiResponse<string>.Fail(message ?? MessageConstants.Get(MessageConstants.BadRequest))),
            "Conflict" => Conflict(ApiResponse<string>.Fail(message ?? MessageConstants.Get(MessageConstants.BadRequest), 409)),
            "Unauthorized" => Unauthorized(ApiResponse<string>.Unauthorized(message ?? MessageConstants.Get(MessageConstants.Unauthorized))),
            "Forbidden" => StatusCode(403, ApiResponse<string>.Forbidden(message ?? MessageConstants.Get(MessageConstants.Forbidden))),
            _ => BadRequest(ApiResponse<string>.Fail(message ?? MessageConstants.Get(MessageConstants.InternalError)))
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

/// <summary>
/// Generic Base API Controller for standard CRUD operations
/// </summary>
/// <typeparam name="TResponse">DTO for response</typeparam>
/// <typeparam name="TCreateDto">DTO for create operation</typeparam>
/// <typeparam name="TUpdateDto">DTO for update operation</typeparam>
public abstract class BaseApiController<TResponse, TCreateDto, TUpdateDto> : BaseApiController
{
    protected BaseApiController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Generic Search/Paged query
    /// </summary>
    [HttpPost("search")]
    public virtual async Task<IActionResult> Search([FromBody] RequestDTO request)
    {
        var pageIndex = request.PageIndex ?? AppConstants.Paging.DefaultPageNumber;
        var pageSize = request.PageSize ?? AppConstants.Paging.DefaultPageSize;
        
        // Default search extraction
        string? search = request.Searching?.FirstOrDefault(s => 
            s.Field?.ToLower() == "name" || s.Field?.ToLower() == "search" || s.Field?.ToLower() == "code")?.Value;

        // Extract other filters (exclude the ones used for text search)
        var filters = request.Searching?.Where(s => 
            s.Field?.ToLower() != "name" && s.Field?.ToLower() != "search" && s.Field?.ToLower() != "code"
        ).ToList();

        var result = await Mediator.Send(CreatePagedQuery(pageIndex, pageSize, search, request.SortDTO, filters));
        return HandleResult(result);
    }

    /// <summary>
    /// Generic Get by Code
    /// </summary>
    [HttpGet("{code}")]
    public virtual async Task<IActionResult> Get(string code)
    {
        var result = await Mediator.Send(CreateGetByCodeQuery(code));
        return HandleResult(result);
    }

    /// <summary>
    /// Generic Create
    /// </summary>
    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] RequestDTO<TCreateDto> request)
    {
        if (request.PostObject == null)
            return BadRequest(ApiResponse<string>.Fail(MessageConstants.Get(MessageConstants.BadRequest)));

        var result = await Mediator.Send(CreateCreateCommand(request.PostObject));
        
        return HandleCreated(result, nameof(Get), GetRouteValues(result.Value), MessageConstants.Get(MessageConstants.Created));
    }

    /// <summary>
    /// Generic Update
    /// </summary>
    [HttpPut("{code}")]
    public virtual async Task<IActionResult> Update(string code, [FromBody] RequestDTO<TUpdateDto> request)
    {
        if (request.PostObject == null)
            return BadRequest(ApiResponse<string>.Fail(MessageConstants.Get(MessageConstants.BadRequest)));

        var result = await Mediator.Send(CreateUpdateCommand(code, request.PostObject));
        return HandleResult(result, MessageConstants.Get(MessageConstants.Updated));
    }

    /// <summary>
    /// Generic Delete
    /// </summary>
    [HttpDelete("{code}")]
    public virtual async Task<IActionResult> Delete(string code)
    {
        var result = await Mediator.Send(CreateDeleteCommand(code));
        return HandleDelete(result);
    }

    // Abstract factory methods to create specific MediatR requests
    protected abstract IRequest<Result<PagedResult<TResponse>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters);
    protected abstract IRequest<Result<TResponse>> CreateGetByCodeQuery(string code);
    protected abstract IRequest<Result<TResponse>> CreateCreateCommand(TCreateDto dto);
    protected abstract IRequest<Result<TResponse>> CreateUpdateCommand(string code, TUpdateDto dto);
    protected abstract IRequest<Result> CreateDeleteCommand(string code);
    
    // Helper to get route values for CreatedAtAction
    protected virtual object GetRouteValues(TResponse? value)
    {
        var codeProp = value?.GetType().GetProperty("Code");
        return new { code = codeProp?.GetValue(value) ?? "" };
    }
}
