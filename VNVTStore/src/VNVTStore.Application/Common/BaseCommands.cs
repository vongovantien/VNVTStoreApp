using MediatR;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Common;

/// <summary>
/// Base Query cho việc lấy single entity theo Code - REUSABLE
/// </summary>
public abstract record GetByCodeQuery<TResponse>(string Code) : IRequest<Result<TResponse>>;

/// <summary>
/// Base Query cho việc lấy danh sách với phân trang - REUSABLE
/// </summary>
public abstract record GetPagedQuery<TResponse>(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    SortDTO? SortDTO = null
) : IRequest<Result<PagedResult<TResponse>>>;

/// <summary>
/// Base Query cho việc lấy tất cả items - REUSABLE
/// </summary>
public abstract record GetAllQuery<TResponse> : IRequest<Result<IEnumerable<TResponse>>>;

/// <summary>
/// Base Command cho Create operation với DTO - REUSABLE
/// </summary>
public abstract record CreateCommand<TDto, TResponse>(TDto Dto) : IRequest<Result<TResponse>>;

/// <summary>
/// Base Command cho Update operation với DTO - REUSABLE
/// </summary>
public abstract record UpdateCommand<TDto, TResponse>(string Code, TDto Dto) : IRequest<Result<TResponse>>;

/// <summary>
/// Base Command cho Delete operation - REUSABLE
/// </summary>
public abstract record DeleteCommand(string Code) : IRequest<Result>;
