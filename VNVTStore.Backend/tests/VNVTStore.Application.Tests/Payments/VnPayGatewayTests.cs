using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Payments;
using VNVTStore.Domain.Enums;
using VNVTStore.Infrastructure.Services.Payments;
using Xunit;

namespace VNVTStore.Application.Tests.Payments;

public class VnPayGatewayTests
{
    private readonly Mock<ISecretConfigurationService> _secretConfigMock;
    private readonly Mock<ILogger<VnPayGateway>> _loggerMock;
    private readonly VnPayGateway _gateway;

    public VnPayGatewayTests()
    {
        _secretConfigMock = new Mock<ISecretConfigurationService>();
        _loggerMock = new Mock<ILogger<VnPayGateway>>();
        _gateway = new VnPayGateway(_secretConfigMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void PaymentReference_BuildAndParse_RoundTrip_Works()
    {
        var now = new DateTime(2026, 10, 4, 15, 30, 0, DateTimeKind.Utc);
        var paymentCode = "PAY123456";

        var txnRef = PaymentReference.Build(paymentCode, now);
        Assert.StartsWith(paymentCode, txnRef);

        var ok = PaymentReference.TryParse(txnRef, out var parsed);
        Assert.True(ok);
        Assert.Equal(paymentCode, parsed);
    }

    [Fact]
    public async Task CreatePaymentUrlAsync_BuildsSignedUrl()
    {
        _secretConfigMock.Setup(s => s.GetSecretAsync(PaymentSecretKeys.VnPayTmnCode))
            .ReturnsAsync("DEMOTMN1");
        _secretConfigMock.Setup(s => s.GetSecretAsync(PaymentSecretKeys.VnPayHashSecret))
            .ReturnsAsync("RAZOPYIHIRZAKJYGFKCFJWJFDGEYTGKL");
        _secretConfigMock.Setup(s => s.GetSecretAsync(PaymentSecretKeys.VnPayBaseUrl))
            .ReturnsAsync("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");

        var request = new PaymentGatewayRequest(
            TxnRef: "PAY00120261004150000",
            Amount: 100000,
            OrderCode: "ORD001",
            OrderInfo: "Thanh toan don hang ORD001",
            ClientIp: "127.0.0.1",
            ReturnUrl: "http://localhost:5173/payment/result",
            IpnUrl: "http://localhost:5000/api/v1/payments/vnpay/ipn"
        );

        var url = await _gateway.CreatePaymentUrlAsync(request);

        Assert.Contains("vnp_TmnCode=DEMOTMN1", url);
        Assert.Contains("vnp_Amount=10000000", url); // VND x 100
        Assert.Contains("vnp_SecureHash=", url);
        Assert.Contains("vnp_TxnRef=PAY00120261004150000", url);
    }

    [Fact]
    public async Task VerifyCallbackAsync_WithInvalidSignature_Fails()
    {
        _secretConfigMock.Setup(s => s.GetSecretAsync(PaymentSecretKeys.VnPayHashSecret))
            .ReturnsAsync("RAZOPYIHIRZAKJYGFKCFJWJFDGEYTGKL");

        var callbackData = new Dictionary<string, string>
        {
            { "vnp_Amount", "10000000" },
            { "vnp_ResponseCode", "00" },
            { "vnp_TxnRef", "PAY001" },
            { "vnp_SecureHash", "invalid_hash_string" }
        };

        var result = await _gateway.VerifyCallbackAsync(callbackData);

        Assert.False(result.IsValidSignature);
        Assert.False(result.IsSuccess);
    }
}
