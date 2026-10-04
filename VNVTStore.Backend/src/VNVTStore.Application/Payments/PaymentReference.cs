using System.Globalization;

namespace VNVTStore.Application.Payments;

/// <summary>
/// Builds/parses the per-attempt transaction reference sent to gateways.
/// Format: {paymentCode}{yyyyMMddHHmmss}. VNPay/MoMo require a new unique reference for
/// every attempt, while we keep a single TblPayment per order (unique OrderCode index).
/// </summary>
public static class PaymentReference
{
    private const string TimestampFormat = "yyyyMMddHHmmss";
    private const int TimestampLength = 14;

    public static string Build(string paymentCode, DateTime utcNow) =>
        paymentCode + utcNow.ToString(TimestampFormat, CultureInfo.InvariantCulture);

    public static bool TryParse(string? txnRef, out string paymentCode)
    {
        paymentCode = string.Empty;
        if (string.IsNullOrWhiteSpace(txnRef) || txnRef.Length <= TimestampLength) return false;

        var stamp = txnRef[^TimestampLength..];
        if (!DateTime.TryParseExact(stamp, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            return false;

        paymentCode = txnRef[..^TimestampLength];
        return true;
    }
}

/// <summary>
/// System secret keys used by the payment module (managed in Admin → System Secrets).
/// </summary>
public static class PaymentSecretKeys
{
    public const string VnPayTmnCode = "VNPAY_TMN_CODE";
    public const string VnPayHashSecret = "VNPAY_HASH_SECRET";
    public const string VnPayBaseUrl = "VNPAY_BASE_URL";

    public const string MoMoPartnerCode = "MOMO_PARTNER_CODE";
    public const string MoMoAccessKey = "MOMO_ACCESS_KEY";
    public const string MoMoSecretKey = "MOMO_SECRET_KEY";
    public const string MoMoEndpoint = "MOMO_ENDPOINT";

    public const string ReturnUrl = "PAYMENT_RETURN_URL";
    public const string IpnBaseUrl = "PAYMENT_IPN_BASE_URL";
    public const string TimeoutMinutes = "PAYMENT_TIMEOUT_MINUTES";

    public const string FrontendUrl = "FRONTEND_URL";
}
