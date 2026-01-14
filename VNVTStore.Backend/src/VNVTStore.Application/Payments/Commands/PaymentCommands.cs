using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Payments.Commands;

public record ProcessPaymentCommand(
    string orderCode,
    PaymentMethod paymentMethod,
    decimal amount
) : IRequest<Result<PaymentDto>>;

public record UpdatePaymentStatusCommand(
    string paymentCode,
    PaymentStatus status,
    string? transactionId
) : IRequest<Result<PaymentDto>>;
