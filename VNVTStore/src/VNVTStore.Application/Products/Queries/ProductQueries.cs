using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Products.Queries;

/// <summary>
/// Query lấy danh sách Products với phân trang
/// </summary>
public record GetProductsQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    SortDTO? SortDTO = null
) : GetPagedQuery<ProductDto>(PageIndex, PageSize, Search, SortDTO);

/// <summary>
/// Query lấy Product theo Code
/// </summary>
public record GetProductByCodeQuery(string Code) : GetByCodeQuery<ProductDto>(Code);
