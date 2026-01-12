using MediatR;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Common;

/// <summary>
/// Base Query cho việc lấy single entity theo Code - REUSABLE
/// </summary>
public record GetByCodeQuery<TResponse>(string Code) : IRequest<Result<TResponse>>;

/// <summary>
/// Base Query cho việc lấy danh sách với phân trang - REUSABLE
/// </summary>
public record GetPagedQuery<TResponse>(
    int PageIndex = AppConstants.Paging.DefaultPageNumber,
    int PageSize = AppConstants.Paging.DefaultPageSize,
    string? Search = null,
    SortDTO? SortDTO = null,
    List<SearchDTO>? Filters = null,
    List<string>? Fields = null
) : IRequest<Result<PagedResult<TResponse>>>;

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
