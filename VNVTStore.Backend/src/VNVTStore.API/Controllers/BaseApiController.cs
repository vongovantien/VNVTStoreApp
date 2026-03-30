using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using Serilog;

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

        // Debug log
        if (result.Value != null)
        {
             Log.Debug("[HandleResult] Result Type: {TypeName}. Data: {Data}", typeof(T).Name, Newtonsoft.Json.JsonConvert.SerializeObject(result.Value));
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
        // Debug log
        Log.Warning("[HandleError] Code: {ErrorCode}. Message: {ErrorMessage}", error.Code, error.Message);

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
        return User.IsInRole(nameof(UserRole.Admin));
    }
}

/// <summary>
/// Generic Base API Controller for standard CRUD operations
/// </summary>
/// <typeparam name="TEntity">Domain Entity type</typeparam>
/// <typeparam name="TResponse">DTO for response</typeparam>
/// <typeparam name="TCreateDto">DTO for create operation</typeparam>
/// <typeparam name="TUpdateDto">DTO for update operation</typeparam>
public abstract class BaseApiController<TEntity, TResponse, TCreateDto, TUpdateDto> : BaseApiController
    where TEntity : class, VNVTStore.Domain.Interfaces.IEntity
    where TResponse : class, IBaseDto
{
    protected BaseApiController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("search")]
    public virtual async Task<IActionResult> Search([FromBody] RequestDTO request)
    {
        var pageIndex = request.PageIndex ?? AppConstants.Paging.DefaultPageNumber;
        var pageSize = request.PageSize ?? AppConstants.Paging.DefaultPageSize;

        // If not admin, force IsActive = true for safety on public-facing generic search
        if (!IsAdmin())
        {
            request.Searching ??= new List<SearchDTO>();
            if (!request.Searching.Any(s => s.SearchField.Equals("IsActive", StringComparison.OrdinalIgnoreCase)))
            {
                request.Searching.Add(new SearchDTO 
                { 
                    SearchField = "IsActive", 
                    SearchValue = true, 
                    SearchCondition = SearchCondition.Equal 
                });
            }
        }

        var result = await Mediator.Send(CreatePagedQuery(pageIndex, pageSize, request.SortDTO, request.Searching, request.Fields));
        return HandleResult(result);
    }

    /// <summary>
    /// Generic Get by Code
    /// </summary>
    [HttpGet("{code}")]
    public virtual async Task<IActionResult> Get(string code, [FromQuery] bool includeChildren = false)
    {
        var result = await Mediator.Send(CreateGetByCodeQuery(code, includeChildren));
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

    /// <summary>
    /// Generic Delete Multiple
    /// </summary>
    [HttpPost("delete-multiple")]
    public virtual async Task<IActionResult> DeleteMultiple([FromBody] List<string> codes)
    {
        var result = await Mediator.Send(CreateDeleteMultipleCommand(codes));
        return HandleDelete(result);
    }

    /// <summary>
    /// Generic Export to Excel
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public virtual async Task<IActionResult> Export()
    {
        var result = await Mediator.Send(new ExportAllQuery<TResponse>());
        if (result.IsFailure) return HandleError(result.Error!);
        
        var fileName = $"{typeof(TEntity).Name.Replace("Tbl", "")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Generic Get Import Template
    /// </summary>
    [HttpGet("template")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public virtual async Task<IActionResult> GetTemplate()
    {
        var result = await Mediator.Send(new GetTemplateQuery<TCreateDto>());
        if (result.IsFailure) return HandleError(result.Error!);
        
        var fileName = $"{typeof(TEntity).Name.Replace("Tbl", "")}_Template.xlsx";
        return File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Generic Import from Excel
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [Consumes("multipart/form-data")]
    public virtual async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0) 
            return BadRequest(ApiResponse<string>.Fail(MessageConstants.Get(MessageConstants.BadRequest)));
        
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;
        
        var result = await Mediator.Send(new ImportCommand<TCreateDto, TResponse>(stream));
        return HandleResult(result);
    }

    // Default implementations of factory methods
    protected virtual IRequest<Result<PagedResult<TResponse>>> CreatePagedQuery(int pageIndex, int pageSize, SortDTO? sort, List<SearchDTO>? filters, List<string>? fields = null)
        => new GetPagedQuery<TResponse> { PageIndex = pageIndex, PageSize = pageSize, SortDTO = sort, Searching = filters, Fields = fields };

    protected virtual IRequest<Result<TResponse>> CreateGetByCodeQuery(string code, bool includeChildren)
        => new GetByCodeQuery<TResponse>(code, includeChildren);

    protected virtual IRequest<Result<TResponse>> CreateCreateCommand(TCreateDto dto)
        => new CreateCommand<TCreateDto, TResponse>(dto);

    protected virtual IRequest<Result<TResponse>> CreateUpdateCommand(string code, TUpdateDto dto)
        => new UpdateCommand<TUpdateDto, TResponse>(code, dto);

    protected virtual IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TEntity>(code);
    
    protected virtual IRequest<Result> CreateDeleteMultipleCommand(List<string> codes)
        => new DeleteMultipleCommand<TEntity>(codes);
    
    // Helper to get route values for CreatedAtAction
    protected virtual object GetRouteValues(TResponse? value)
    {
        var codeProp = value?.GetType().GetProperty("Code");
        return new { code = codeProp?.GetValue(value) ?? "" };
    }
}
