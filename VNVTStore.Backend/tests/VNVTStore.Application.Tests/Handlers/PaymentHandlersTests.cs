using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;

    public PaymentHandlersTests()
    {
        _paymentRepositoryMock = new Mock<IRepository<TblPayment>>();
        _orderRepositoryMock = new Mock<IRepository<TblOrder>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_ProcessPayment_Success_ShouldReturnPaymentDto()
    {
        // Arrange
        var command = new ProcessPaymentCommand("ORD001", PaymentMethod.BankTransfer, 1000);
        var handler = new PaymentHandlers(
            _paymentRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _currentUserMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );

        var order = TblOrder.Create("USER001", "ADDR001", 1000, 0, 0, null);
        _orderRepositoryMock.Setup(x => x.GetByCodeAsync("ORD001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        
        _currentUserMock.Setup(x => x.UserCode).Returns("USER001");
        _mapperMock.Setup(x => x.Map<PaymentDto>(It.IsAny<TblPayment>())).Returns(new PaymentDto { OrderCode = "ORD001", Amount = 1000 });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.IsSuccess) 
            throw new Exception($"Handler failed with error: {result.Error?.Message}");

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
        var handler = new PaymentHandlers(_paymentRepositoryMock.Object, _orderRepositoryMock.Object, _currentUserMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);

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

        var handler = new PaymentHandlers(_paymentRepositoryMock.Object, _orderRepositoryMock.Object, _currentUserMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);

        // Act
        var result = await handler.Handle(new GetMyPaymentsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }
}
