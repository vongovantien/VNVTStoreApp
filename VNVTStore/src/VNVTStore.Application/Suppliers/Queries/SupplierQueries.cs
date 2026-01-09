using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Suppliers.Queries;

public record GetAllSuppliersQuery(
    int PageIndex = AppConstants.Paging.DefaultPageNumber,
    int PageSize = AppConstants.Paging.DefaultPageSize,
    string? Search = null,
    bool? IsActive = null
) : GetPagedQuery<SupplierDto>(PageIndex, PageSize, Search);

public record GetSupplierByCodeQuery(string SupplierCode) : GetByCodeQuery<SupplierDto>(SupplierCode);
