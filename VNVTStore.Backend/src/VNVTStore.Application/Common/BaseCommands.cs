using MediatR;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;

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
public record CreateCommand<TDto, TResponse>(TDto Dto) : IRequest<Result<TResponse>>;

/// <summary>
/// Base Command cho Update operation với DTO - REUSABLE
/// </summary>
public record UpdateCommand<TDto, TResponse>(string Code, TDto Dto) : IRequest<Result<TResponse>>;

/// <summary>
/// Base Command cho Delete operation - REUSABLE
/// Phải có TMarker để MediatR phân biệt được handler của từng entity (TMarker thường là Entity type)
/// </summary>
public record DeleteCommand<TMarker>(string Code) : IRequest<Result>;

/// <summary>
/// Base Command cho Delete Multiple operations - REUSABLE
/// </summary>
public record DeleteMultipleCommand<TMarker>(List<string> Codes) : IRequest<Result>;

/// <summary>
/// Base Query to get simple stats (Total, Active) - REUSABLE
/// </summary>
public record GetStatsQuery<TMarker> : IRequest<Result<EntityStatsDto>>;
