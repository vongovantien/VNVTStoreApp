using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Payments.Commands;

public record ProcessPaymentCommand(
    string OrderCode,
    string PaymentMethod,
    decimal Amount
) : IRequest<Result<PaymentDto>>;

public record UpdatePaymentStatusCommand(
    string PaymentCode,
    string Status, // Pending, Completed, Failed
    string? TransactionId
) : IRequest<Result<PaymentDto>>;
