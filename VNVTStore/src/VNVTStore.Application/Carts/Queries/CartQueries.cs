using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Carts.Queries;

public record GetCartQuery(string UserCode) : IRequest<Result<CartDto>>;
public record GetMyCartQuery(string UserCode) : IRequest<Result<CartDto>>;
