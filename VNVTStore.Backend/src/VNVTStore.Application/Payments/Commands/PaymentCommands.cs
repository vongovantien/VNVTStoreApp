using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Payments.Commands;

/// <summary>
/// Create or reset the payment record of an order. Amount is always taken from the order.
/// </summary>
public record ProcessPaymentCommand(
    string orderCode,
    PaymentMethod paymentMethod
) : IRequest<Result<PaymentDto>>;

public record UpdatePaymentStatusCommand(
    string paymentCode,
    PaymentStatus status,
    string? transactionId
) : IRequest<Result<PaymentDto>>;

/// <summary>
/// Build a VNPay/MoMo checkout URL for an order.
/// </summary>
/// <param name="OrderCode">Order to pay.</param>
/// <param name="ClientIp">Customer IP (VNPay requirement).</param>
/// <param name="PaymentMethod">Optional: switch the order to another online method before paying.</param>
public record CreatePaymentUrlCommand(
    string OrderCode,
    string ClientIp,
    PaymentMethod? PaymentMethod = null
) : IRequest<Result<PaymentUrlDto>>;

/// <summary>
/// Verify and apply a gateway callback. Used by both IPN (server-to-server) and the browser return URL.
/// Idempotent: processing the same successful callback twice has no additional effect.
/// </summary>
public record ConfirmGatewayPaymentCommand(
    PaymentMethod Gateway,
    IReadOnlyDictionary<string, string> Data
) : IRequest<Result<PaymentConfirmationDto>>;

/// <summary>
/// Cancel online-payment orders that stayed unpaid longer than PAYMENT_TIMEOUT_MINUTES and restore stock.
/// Returns the number of cancelled orders.
/// </summary>
public record CancelExpiredOnlinePaymentsCommand : IRequest<int>;
