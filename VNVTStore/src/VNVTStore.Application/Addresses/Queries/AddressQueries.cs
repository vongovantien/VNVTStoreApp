using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Addresses.Queries;

public record GetUserAddressesQuery(string UserCode) : IRequest<Result<IEnumerable<AddressDto>>>;

public record GetAddressByCodeQuery(string AddressCode) : IRequest<Result<AddressDto>>;
