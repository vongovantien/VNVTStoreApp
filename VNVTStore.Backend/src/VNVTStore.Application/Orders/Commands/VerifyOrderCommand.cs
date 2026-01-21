using MediatR;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Orders.Commands;

public record VerifyOrderCommand(string Token) : IRequest<Result<string>>;
