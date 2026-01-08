using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Suppliers.Queries;

public record GetAllSuppliersQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null
) : IRequest<Result<PagedResult<SupplierDto>>>;

public record GetSupplierByCodeQuery(string SupplierCode) : IRequest<Result<SupplierDto>>;
