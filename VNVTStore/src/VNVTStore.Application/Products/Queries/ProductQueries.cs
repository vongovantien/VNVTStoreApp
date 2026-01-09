using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Products.Queries;

public record GetProductsQuery(
    int PageIndex = AppConstants.Paging.DefaultPageNumber,
    int PageSize = AppConstants.Paging.DefaultPageSize,
    string? Search = null,
    SortDTO? SortDTO = null,
    string? CategoryCode = null
) : GetPagedQuery<ProductDto>(PageIndex, PageSize, Search, SortDTO);

public record GetProductByCodeQuery(string Code) : GetByCodeQuery<ProductDto>(Code);
