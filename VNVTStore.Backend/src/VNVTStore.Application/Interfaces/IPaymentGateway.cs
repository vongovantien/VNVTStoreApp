using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Interfaces;

/// <summary>
/// Data needed by a gateway to build a payment URL.
/// </summary>
/// <param name="TxnRef">Unique reference for this payment attempt (see <c>PaymentReference</c>).</param>
/// <param name="Amount">Amount in VND.</param>
/// <param name="OrderCode">Store order code.</param>
/// <param name="OrderInfo">Human-readable description shown on the gateway page.</param>
/// <param name="ClientIp">Customer IP address (required by VNPay).</param>
/// <param name="ReturnUrl">Browser redirect URL after payment.</param>
/// <param name="IpnUrl">Server-to-server notification URL (used by MoMo; VNPay configures it in the merchant portal).</param>
public record PaymentGatewayRequest(
    string TxnRef,
    decimal Amount,
    string OrderCode,
    string OrderInfo,
    string ClientIp,
    string ReturnUrl,
    string IpnUrl);

/// <summary>
/// Normalised result of verifying a gateway callback (return URL or IPN).
/// </summary>
public record PaymentGatewayResult(
    bool IsValidSignature,
    bool IsSuccess,
    string? TxnRef,
    decimal Amount,
    string? GatewayTransactionId,
    string? ResponseCode,
    string? Message)
{
    public static PaymentGatewayResult InvalidSignature() =>
        new(false, false, null, 0, null, null, "Invalid signature");
}

public interface IPaymentGateway
{
    PaymentMethod Method { get; }

    /// <summary>Whether all required credentials are configured in system secrets.</summary>
    Task<bool> IsConfiguredAsync();

    Task<string> CreatePaymentUrlAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default);

    Task<PaymentGatewayResult> VerifyCallbackAsync(IReadOnlyDictionary<string, string> data);
}

public interface IPaymentGatewayFactory
{
    /// <summary>Returns the gateway for a method, or null if the method is not an online gateway.</summary>
    IPaymentGateway? Get(PaymentMethod method);
}
