using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Addresses.Commands;

public record CreateAddressCommand(
    string UserCode,
    string AddressLine,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    bool IsDefault
) : IRequest<Result<AddressDto>>;

public record UpdateAddressCommand(
    string AddressCode,
    string UserCode,
    string? AddressLine,
    string? City,
    string? State,
    string? PostalCode,
    bool? IsDefault
) : IRequest<Result<AddressDto>>;

public record DeleteAddressCommand(string AddressCode, string UserCode) : IRequest<Result<bool>>;

public record SetDefaultAddressCommand(string AddressCode, string UserCode) : IRequest<Result<bool>>;
