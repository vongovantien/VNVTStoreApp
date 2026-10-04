using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Payments.Commands;
using VNVTStore.Application.Payments.Handlers;
using VNVTStore.Application.Payments.Queries;
using VNVTStore.Application.Tests.Helpers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Application.Tests.Handlers;

public class PaymentHandlersTests
{
    private readonly Mock<IRepository<TblPayment>> _paymentRepositoryMock;
    private readonly Mock<IRepository<TblOrder>> _orderRepositoryMock;
    private readonly Mock<IRepository<TblProduct>> _productRepositoryMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPaymentGatewayFactory> _gatewayFactoryMock;
    private readonly Mock<ISecretConfigurationService> _secretConfigMock;
    private readonly Mock<IBaseUrlService> _baseUrlServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<PaymentHandlers>> _loggerMock;

    public PaymentHandlersTests()
    {
        _paymentRepositoryMock = new Mock<IRepository<TblPayment>>();
        _orderRepositoryMock = new Mock<IRepository<TblOrder>>();
        _productRepositoryMock = new Mock<IRepository<TblProduct>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _gatewayFactoryMock = new Mock<IPaymentGatewayFactory>();
        _secretConfigMock = new Mock<ISecretConfigurationService>();
        _baseUrlServiceMock = new Mock<IBaseUrlService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<PaymentHandlers>>();

        _paymentRepositoryMock.Setup(x => x.AsQueryable())
            .Returns(TestingUtils.CreateMockDbSet(new List<TblPayment>()).Object);
        _orderRepositoryMock.Setup(x => x.AsQueryable())
            .Returns(TestingUtils.CreateMockDbSet(new List<TblOrder>()).Object);
    }

    private PaymentHandlers CreateHandler()
    {
        return new PaymentHandlers(
            _paymentRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object,
            _currentUserMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _gatewayFactoryMock.Object,
            _secretConfigMock.Object,
            _baseUrlServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ProcessPayment_Success_ShouldReturnPaymentDto()
    {
        // Arrange
        var command = new ProcessPaymentCommand("ORD001", PaymentMethod.BankTransfer);
        var handler = CreateHandler();

        var order = TblOrder.Create("USER001", "ADDR001", 1000, 0, 0, null);
        _orderRepositoryMock.Setup(x => x.GetByCodeAsync("ORD001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        
        _currentUserMock.Setup(x => x.UserCode).Returns("USER001");
        _mapperMock.Setup(x => x.Map<PaymentDto>(It.IsAny<TblPayment>())).Returns(new PaymentDto { OrderCode = "ORD001", Amount = 1000 });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("ORD001", result.Value!.OrderCode);
        _paymentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TblPayment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdatePaymentStatus_ToCompleted_ShouldUpdateOrderToPaid()
    {
        // Arrange
        var command = new UpdatePaymentStatusCommand("PAY001", PaymentStatus.Completed, "TXN123");
        var handler = CreateHandler();

        var payment = TblPayment.Create("ORD001", 1000, PaymentMethod.BankTransfer);
        var order = TblOrder.Create("USER001", "ADDR001", 1000, 0, 0, null);

        _paymentRepositoryMock.Setup(x => x.GetByCodeAsync("PAY001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _orderRepositoryMock.Setup(x => x.GetByCodeAsync("ORD001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public async Task Handle_GetMyPayments_ShouldReturnFilteredPayments()
    {
        // Arrange
        var userCode = "USER001";
        _currentUserMock.Setup(x => x.UserCode).Returns(userCode);

        var order = TblOrder.Create(userCode, "ADDR001", 1000, 0, 0, null);
        var payment = TblPayment.Create("ORD001", 1000, PaymentMethod.Cash);
        
        typeof(TblPayment).GetProperty(nameof(TblPayment.OrderCodeNavigation))?
            .SetValue(payment, order);

        var payments = new List<TblPayment> { payment };
        var mockDbSet = TestingUtils.CreateMockDbSet(payments);
        _paymentRepositoryMock.Setup(x => x.AsQueryable()).Returns(mockDbSet.Object);
        _mapperMock.Setup(x => x.Map<IEnumerable<PaymentDto>>(It.IsAny<IEnumerable<TblPayment>>()))
            .Returns(new List<PaymentDto> { new PaymentDto { OrderCode = "ORD001" } });

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetMyPaymentsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task Handle_CreatePaymentUrl_ShouldCallGateway_WhenConfigured()
    {
        // Arrange
        var order = TblOrder.Create("USER001", "ADDR001", 500000, 30000, 0, null);
        _orderRepositoryMock.Setup(x => x.GetByCodeAsync("ORD001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _currentUserMock.Setup(x => x.UserCode).Returns("USER001");

        var gatewayMock = new Mock<IPaymentGateway>();
        gatewayMock.Setup(g => g.Method).Returns(PaymentMethod.VnPay);
        gatewayMock.Setup(g => g.IsConfiguredAsync()).ReturnsAsync(true);
        gatewayMock.Setup(g => g.CreatePaymentUrlAsync(It.IsAny<PaymentGatewayRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://sandbox.vnpayment.vn/test-pay-url");

        _gatewayFactoryMock.Setup(f => f.Get(PaymentMethod.VnPay)).Returns(gatewayMock.Object);
        _baseUrlServiceMock.Setup(b => b.GetBaseUrl()).Returns("http://localhost:5000");

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new CreatePaymentUrlCommand("ORD001", "127.0.0.1", PaymentMethod.VnPay), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("https://sandbox.vnpayment.vn/test-pay-url", result.Value!.PaymentUrl);
    }
}
