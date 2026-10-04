using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Payments;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Infrastructure.Services.Payments;

public class MoMoGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly ISecretConfigurationService _secretConfig;
    private readonly ILogger<MoMoGateway> _logger;

    public PaymentMethod Method => PaymentMethod.MoMo;

    public MoMoGateway(HttpClient httpClient, ISecretConfigurationService secretConfig, ILogger<MoMoGateway> logger)
    {
        _httpClient = httpClient;
        _secretConfig = secretConfig;
        _logger = logger;
    }

    public async Task<bool> IsConfiguredAsync()
    {
        var partnerCode = await _secretConfig.GetSecretAsync(PaymentSecretKeys.MoMoPartnerCode);
        var accessKey = await _secretConfig.GetSecretAsync(PaymentSecretKeys.MoMoAccessKey);
        var secretKey = await _secretConfig.GetSecretAsync(PaymentSecretKeys.MoMoSecretKey);
        return !string.IsNullOrWhiteSpace(partnerCode) &&
               !string.IsNullOrWhiteSpace(accessKey) &&
               !string.IsNullOrWhiteSpace(secretKey);
    }

    public async Task<string> CreatePaymentUrlAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        var partnerCode = await _secretConfig.GetSecretAsync(PaymentSecretKeys.MoMoPartnerCode);
        var accessKey = await _secretConfig.GetSecretAsync(PaymentSecretKeys.MoMoAccessKey);
        var secretKey = await _secretConfig.GetSecretAsync(PaymentSecretKeys.MoMoSecretKey);
        var endpoint = await _secretConfig.GetSecretAsync(PaymentSecretKeys.MoMoEndpoint)
                       ?? "https://test-payment.momo.vn/v2/gateway/api/create";

        if (string.IsNullOrWhiteSpace(partnerCode) ||
            string.IsNullOrWhiteSpace(accessKey) ||
            string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("MoMo credentials are not configured in system secrets.");
        }

        var requestId = Guid.NewGuid().ToString("N");
        var amount = (long)Math.Round(request.Amount, MidpointRounding.AwayFromZero);
        var orderId = request.TxnRef;
        var orderInfo = request.OrderInfo;
        var redirectUrl = request.ReturnUrl;
        var ipnUrl = request.IpnUrl;
        var requestType = "captureWallet";
        var extraData = "";

        // MoMo creation signature pattern:
        // accessKey=$accessKey&amount=$amount&extraData=$extraData&ipnUrl=$ipnUrl&orderId=$orderId&orderInfo=$orderInfo&partnerCode=$partnerCode&redirectUrl=$redirectUrl&requestId=$requestId&requestType=$requestType
        var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
        var signature = HmacSha256(secretKey, rawHash);

        var payload = new MoMoCreatePaymentRequest
        {
            PartnerCode = partnerCode,
            RequestId = requestId,
            Amount = amount,
            OrderId = orderId,
            OrderInfo = orderInfo,
            RedirectUrl = redirectUrl,
            IpnUrl = ipnUrl,
            RequestType = requestType,
            ExtraData = extraData,
            Lang = "vi",
            Signature = signature
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("MoMo Create Payment HTTP error ({StatusCode}): {Response}", response.StatusCode, err);
            throw new InvalidOperationException($"MoMo Create Payment API error ({response.StatusCode}): {err}");
        }

        var result = await response.Content.ReadFromJsonAsync<MoMoCreatePaymentResponse>(cancellationToken: cancellationToken);
        if (result == null || string.IsNullOrWhiteSpace(result.PayUrl))
        {
            _logger.LogError("MoMo API returned invalid response: {Message}, resultCode={ResultCode}", result?.Message, result?.ResultCode);
            throw new InvalidOperationException(result?.Message ?? "Failed to create MoMo payment URL.");
        }

        return result.PayUrl;
    }

    public async Task<PaymentGatewayResult> VerifyCallbackAsync(IReadOnlyDictionary<string, string> data)
    {
        var accessKey = await _secretConfig.GetSecretAsync(PaymentSecretKeys.MoMoAccessKey);
        var secretKey = await _secretConfig.GetSecretAsync(PaymentSecretKeys.MoMoSecretKey);

        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(accessKey))
        {
            _logger.LogError("MoMo secretKey/accessKey missing from system secrets.");
            return PaymentGatewayResult.InvalidSignature();
        }

        if (!data.TryGetValue("signature", out var receivedSignature) || string.IsNullOrWhiteSpace(receivedSignature))
        {
            return PaymentGatewayResult.InvalidSignature();
        }

        data.TryGetValue("partnerCode", out var partnerCode);
        data.TryGetValue("orderId", out var orderId);
        data.TryGetValue("requestId", out var requestId);
        data.TryGetValue("amount", out var amountStr);
        data.TryGetValue("orderInfo", out var orderInfo);
        data.TryGetValue("orderType", out var orderType);
        data.TryGetValue("transId", out var transId);
        data.TryGetValue("resultCode", out var resultCode);
        data.TryGetValue("message", out var message);
        data.TryGetValue("payType", out var payType);
        data.TryGetValue("responseTime", out var responseTime);
        data.TryGetValue("extraData", out var extraData);

        // MoMo Callback Signature Pattern:
        // accessKey=$accessKey&amount=$amount&extraData=$extraData&message=$message&orderId=$orderId&orderInfo=$orderInfo&orderType=$orderType&partnerCode=$partnerCode&payType=$payType&requestId=$requestId&responseTime=$responseTime&resultCode=$resultCode&transId=$transId
        var rawHash = $"accessKey={accessKey}&amount={amountStr}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";
        var computedSignature = HmacSha256(secretKey, rawHash);

        if (!string.Equals(computedSignature, receivedSignature, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("MoMo signature mismatch. Computed: {Computed}, Received: {Received}", computedSignature, receivedSignature);
            return PaymentGatewayResult.InvalidSignature();
        }

        decimal amount = 0;
        if (decimal.TryParse(amountStr, out var rawAmount))
        {
            amount = rawAmount;
        }

        var isSuccess = resultCode == "0";

        return new PaymentGatewayResult(
            IsValidSignature: true,
            IsSuccess: isSuccess,
            TxnRef: orderId,
            Amount: amount,
            GatewayTransactionId: transId,
            ResponseCode: resultCode,
            Message: message ?? (isSuccess ? "Success" : "Failed")
        );
    }

    private static string HmacSha256(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using var hmac = new HMACSHA256(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);
        var hash = new StringBuilder();
        foreach (var b in hashValue)
        {
            hash.Append(b.ToString("x2"));
        }
        return hash.ToString();
    }

    private class MoMoCreatePaymentRequest
    {
        [JsonPropertyName("partnerCode")] public string PartnerCode { get; set; } = null!;
        [JsonPropertyName("requestId")] public string RequestId { get; set; } = null!;
        [JsonPropertyName("amount")] public long Amount { get; set; }
        [JsonPropertyName("orderId")] public string OrderId { get; set; } = null!;
        [JsonPropertyName("orderInfo")] public string OrderInfo { get; set; } = null!;
        [JsonPropertyName("redirectUrl")] public string RedirectUrl { get; set; } = null!;
        [JsonPropertyName("ipnUrl")] public string IpnUrl { get; set; } = null!;
        [JsonPropertyName("requestType")] public string RequestType { get; set; } = null!;
        [JsonPropertyName("extraData")] public string ExtraData { get; set; } = "";
        [JsonPropertyName("lang")] public string Lang { get; set; } = "vi";
        [JsonPropertyName("signature")] public string Signature { get; set; } = null!;
    }

    private class MoMoCreatePaymentResponse
    {
        [JsonPropertyName("partnerCode")] public string? PartnerCode { get; set; }
        [JsonPropertyName("orderId")] public string? OrderId { get; set; }
        [JsonPropertyName("requestId")] public string? RequestId { get; set; }
        [JsonPropertyName("amount")] public long Amount { get; set; }
        [JsonPropertyName("responseTime")] public long ResponseTime { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("resultCode")] public int ResultCode { get; set; }
        [JsonPropertyName("payUrl")] public string? PayUrl { get; set; }
        [JsonPropertyName("shortLink")] public string? ShortLink { get; set; }
    }
}
