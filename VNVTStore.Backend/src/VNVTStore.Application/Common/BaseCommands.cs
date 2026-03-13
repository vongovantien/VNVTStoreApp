using MediatR;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Interfaces;

namespace VNVTStore.Application.Common;

/// <summary>
/// Base Query cho việc lấy single entity theo Code - REUSABLE
/// </summary>
public record GetByCodeQuery<TResponse>(string Code, bool IncludeChildren = false) : IRequest<Result<TResponse>>;

/// <summary>
/// Base Query cho việc lấy danh sách với phân trang - REUSABLE
/// </summary>
public record GetPagedQuery<TResponse>(
    int PageIndex = AppConstants.Paging.DefaultPageNumber,
    int PageSize = AppConstants.Paging.DefaultPageSize,
    SortDTO? SortDTO = null,
    List<SearchDTO>? Searching = null,
    List<string>? Fields = null,
    string? SortField = null,
    bool SortDescending = true
) : IRequest<Result<PagedResult<TResponse>>>
{
    // Secondary constructor to simplify inheritance for basic paged queries
    public GetPagedQuery(int pageIndex, int pageSize) 
        : this(pageIndex, pageSize, null, null, null, null, true) { }
}

/// <summary>
/// Base Query cho việc lấy tất cả items - REUSABLE
/// </summary>
public record GetAllQuery<TResponse> : IRequest<Result<IEnumerable<TResponse>>>;

/// <summary>
/// Base Command cho Create operation với DTO - REUSABLE
/// </summary>
public record CreateCommand<TDto, TResponse>(TDto Dto) : IRequest<Result<TResponse>>, IAuditableCommand
{
    public string? AuditAction => "CREATE_" + typeof(TResponse).Name.Replace("Dto", "").Replace("Catalog", "").ToUpper();
    public string? AuditResourceId => null; // Behavior will try to find 'Code' in response or Dto
}

/// <summary>
/// Base Command cho Update operation với DTO - REUSABLE
/// </summary>
public record UpdateCommand<TDto, TResponse>(string Code, TDto Dto) : IRequest<Result<TResponse>>, IAuditableCommand
{
    public string? AuditAction => "UPDATE_" + typeof(TResponse).Name.Replace("Dto", "").Replace("Catalog", "").ToUpper();
    public string? AuditResourceId => Code;
}

/// <summary>
/// Base Command cho Delete operation - REUSABLE
/// Phải có TMarker để MediatR phân biệt được handler của từng entity (TMarker thường là Entity type)
/// </summary>
public record DeleteCommand<TMarker>(string Code) : IRequest<Result>, IAuditableCommand
{
    public string? AuditAction => "DELETE_" + typeof(TMarker).Name.Replace("Tbl", "").ToUpper();
    public string? AuditResourceId => Code;
}

/// <summary>
/// Base Command cho Delete Multiple operations - REUSABLE
/// </summary>
public record DeleteMultipleCommand<TMarker>(List<string> Codes) : IRequest<Result>, IAuditableCommand
{
    public string? AuditAction => "DELETE_MULTIPLE_" + typeof(TMarker).Name.Replace("Tbl", "").ToUpper();
    public string? AuditResourceId => string.Join(", ", Codes);
}

/// <summary>
/// Base Query to get simple stats (Total, Active) - REUSABLE
/// </summary>
public record GetStatsQuery<TMarker> : IRequest<Result<EntityStatsDto>>;
