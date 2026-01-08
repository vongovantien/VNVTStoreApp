using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Carts.Queries;

public record GetMyCartQuery(string UserCode) : IRequest<Result<CartDto>>;
