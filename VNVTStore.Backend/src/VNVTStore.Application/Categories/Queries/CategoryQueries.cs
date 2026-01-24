using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Constants;

namespace VNVTStore.Application.Categories.Queries;

public record GetCategoriesQuery(
    int PageIndex = AppConstants.Paging.DefaultPageNumber,
    int PageSize = AppConstants.Paging.DefaultPageSize,
    string? Search = null,
    SortDTO? SortDTO = null,
    List<SearchDTO>? Searching = null,
    List<string>? Fields = null
) : GetPagedQuery<CategoryDto>(PageIndex, PageSize, Search, SortDTO, Searching, Fields);

public record GetCategoryByCodeQuery(string Code) : GetByCodeQuery<CategoryDto>(Code);

public record GetCategoryStatsQuery() : IRequest<Result<CategoryStatsDto>>;
