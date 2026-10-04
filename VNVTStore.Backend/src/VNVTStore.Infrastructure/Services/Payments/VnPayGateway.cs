using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Payments;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Infrastructure.Services.Payments;

public class VnPayGateway : IPaymentGateway
{
    private readonly ISecretConfigurationService _secretConfig;
    private readonly ILogger<VnPayGateway> _logger;

    public PaymentMethod Method => PaymentMethod.VnPay;

    public VnPayGateway(ISecretConfigurationService secretConfig, ILogger<VnPayGateway> logger)
    {
        _secretConfig = secretConfig;
        _logger = logger;
    }

    public async Task<bool> IsConfiguredAsync()
    {
        var tmnCode = await _secretConfig.GetSecretAsync(PaymentSecretKeys.VnPayTmnCode);
        var hashSecret = await _secretConfig.GetSecretAsync(PaymentSecretKeys.VnPayHashSecret);
        return !string.IsNullOrWhiteSpace(tmnCode) && !string.IsNullOrWhiteSpace(hashSecret);
    }

    public async Task<string> CreatePaymentUrlAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        var tmnCode = await _secretConfig.GetSecretAsync(PaymentSecretKeys.VnPayTmnCode);
        var hashSecret = await _secretConfig.GetSecretAsync(PaymentSecretKeys.VnPayHashSecret);
        var baseUrl = await _secretConfig.GetSecretAsync(PaymentSecretKeys.VnPayBaseUrl)
                      ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        if (string.IsNullOrWhiteSpace(tmnCode) || string.IsNullOrWhiteSpace(hashSecret))
        {
            throw new InvalidOperationException("VNPay credentials are not configured in system secrets.");
        }

        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

        var amountInCents = (long)Math.Round(request.Amount * 100m, MidpointRounding.AwayFromZero);

        var parameters = new SortedList<string, string>(new VnPayCompare())
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", tmnCode },
            { "vnp_Amount", amountInCents.ToString(CultureInfo.InvariantCulture) },
            { "vnp_CreateDate", vietnamTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", string.IsNullOrWhiteSpace(request.ClientIp) ? "127.0.0.1" : request.ClientIp },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", request.OrderInfo },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", request.ReturnUrl },
            { "vnp_TxnRef", request.TxnRef },
            { "vnp_ExpireDate", vietnamTime.AddMinutes(30).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) }
        };

        var queryBuilder = new StringBuilder();
        var rawDataBuilder = new StringBuilder();

        foreach (var kv in parameters)
        {
            if (string.IsNullOrEmpty(kv.Value)) continue;

            if (queryBuilder.Length > 0)
            {
                queryBuilder.Append('&');
                rawDataBuilder.Append('&');
            }

            queryBuilder.Append(WebUtility.UrlEncode(kv.Key));
            queryBuilder.Append('=');
            queryBuilder.Append(WebUtility.UrlEncode(kv.Value));

            rawDataBuilder.Append(WebUtility.UrlEncode(kv.Key));
            rawDataBuilder.Append('=');
            rawDataBuilder.Append(WebUtility.UrlEncode(kv.Value));
        }

        var secureHash = HmacSha512(hashSecret, rawDataBuilder.ToString());
        queryBuilder.Append("&vnp_SecureHash=");
        queryBuilder.Append(secureHash);

        return $"{baseUrl}?{queryBuilder}";
    }

    public async Task<PaymentGatewayResult> VerifyCallbackAsync(IReadOnlyDictionary<string, string> data)
    {
        var hashSecret = await _secretConfig.GetSecretAsync(PaymentSecretKeys.VnPayHashSecret);
        if (string.IsNullOrWhiteSpace(hashSecret))
        {
            _logger.LogError("VNPay hash secret is missing from system secrets.");
            return PaymentGatewayResult.InvalidSignature();
        }

        if (!data.TryGetValue("vnp_SecureHash", out var receivedHash) || string.IsNullOrWhiteSpace(receivedHash))
        {
            return PaymentGatewayResult.InvalidSignature();
        }

        var sorted = new SortedList<string, string>(new VnPayCompare());
        foreach (var kv in data)
        {
            if (kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase)
                && !kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                && !kv.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(kv.Value))
            {
                sorted.Add(kv.Key, kv.Value);
            }
        }

        var rawData = new StringBuilder();
        foreach (var kv in sorted)
        {
            if (rawData.Length > 0) rawData.Append('&');
            rawData.Append(WebUtility.UrlEncode(kv.Key));
            rawData.Append('=');
            rawData.Append(WebUtility.UrlEncode(kv.Value));
        }

        var computedHash = HmacSha512(hashSecret, rawData.ToString());

        if (!string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("VNPay signature mismatch. Computed: {Computed}, Received: {Received}", computedHash, receivedHash);
            return PaymentGatewayResult.InvalidSignature();
        }

        data.TryGetValue("vnp_ResponseCode", out var responseCode);
        data.TryGetValue("vnp_TransactionStatus", out var transactionStatus);
        data.TryGetValue("vnp_TxnRef", out var txnRef);
        data.TryGetValue("vnp_TransactionNo", out var transactionNo);
        data.TryGetValue("vnp_OrderInfo", out var orderInfo);

        decimal amount = 0;
        if (data.TryGetValue("vnp_Amount", out var amountStr) && long.TryParse(amountStr, out var rawAmount))
        {
            amount = rawAmount / 100m;
        }

        var isSuccess = responseCode == "00" && (transactionStatus == null || transactionStatus == "00");

        return new PaymentGatewayResult(
            IsValidSignature: true,
            IsSuccess: isSuccess,
            TxnRef: txnRef,
            Amount: amount,
            GatewayTransactionId: transactionNo,
            ResponseCode: responseCode,
            Message: isSuccess ? "Success" : $"VNPay response code: {responseCode}"
        );
    }

    private static string HmacSha512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using var hmac = new HMACSHA512(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);
        foreach (var b in hashValue)
        {
            hash.Append(b.ToString("x2"));
        }
        return hash.ToString();
    }

    private class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return CompareInfo.GetCompareInfo("en-US").Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
