using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Addresses.Queries;

public record GetUserAddressesQuery(string UserCode) : GetAllQuery<AddressDto>;

public record GetAddressByCodeQuery(string Code) : GetByCodeQuery<AddressDto>(Code);
