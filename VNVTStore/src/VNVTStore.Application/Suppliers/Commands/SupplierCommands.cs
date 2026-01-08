using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Suppliers.Commands;

public record CreateSupplierCommand(
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    string? TaxCode,
    string? BankAccount,
    string? BankName,
    string? Notes
) : IRequest<Result<SupplierDto>>;

public record UpdateSupplierCommand(
    string SupplierCode,
    string? Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    string? TaxCode,
    string? BankAccount,
    string? BankName,
    string? Notes,
    bool? IsActive
) : IRequest<Result<SupplierDto>>;

public record DeleteSupplierCommand(string SupplierCode) : IRequest<Result<bool>>;
