using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Addresses.Commands;


public record SetDefaultAddressCommand(string Code) : IRequest<Result>;
