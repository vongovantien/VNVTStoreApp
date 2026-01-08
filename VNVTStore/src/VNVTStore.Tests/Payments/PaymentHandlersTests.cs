using AutoMapper;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Payments.Commands;
using VNVTStore.Application.Payments.Queries;
using VNVTStore.Application.Payments.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Tests.Extensions;
using Xunit;

namespace VNVTStore.Tests.Payments;

public class PaymentHandlersTests
{
    private readonly Mock<IRepository<TblPayment>> _paymentRepoMock;
    private readonly Mock<IRepository<TblOrder>> _orderRepoMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly PaymentHandlers _handler;

    public PaymentHandlersTests()
    {
        _paymentRepoMock = new Mock<IRepository<TblPayment>>();
        _orderRepoMock = new Mock<IRepository<TblOrder>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _handler = new PaymentHandlers(
            _paymentRepoMock.Object,
            _orderRepoMock.Object,
            _currentUserMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task ProcessPayment_ValidOrder_ReturnsSuccess()
    {
        // Arrange
        var userCode = "USR001";
        var orderCode = "ORD001";
        var order = new TblOrder { Code = orderCode, UserCode = userCode, FinalAmount = 1000 };
        var paymentDto = new PaymentDto { OrderCode = orderCode, Amount = 1000, Status = "Pending" };

        _currentUserMock.Setup(u => u.UserCode).Returns(userCode);
        _orderRepoMock.Setup(r => r.GetByCodeAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<TblPayment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<PaymentDto>(It.IsAny<TblPayment>())).Returns(paymentDto);

        // Act
        var result = await _handler.Handle(
            new ProcessPaymentCommand(orderCode, "COD", 1000),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Pending", result.Value.Status);
    }

    [Fact]
    public async Task ProcessPayment_NotOwner_ReturnsForbidden()
    {
        // Arrange
        var userCode = "USR001";
        var orderCode = "ORD001";
        var order = new TblOrder { Code = orderCode, UserCode = "USR002" }; // Different user

        _currentUserMock.Setup(u => u.UserCode).Returns(userCode);
        _orderRepoMock.Setup(r => r.GetByCodeAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(
            new ProcessPaymentCommand(orderCode, "COD", 1000),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Forbidden", result.Error!.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetMyPayments_ReturnsList()
    {
        // Arrange
        var userCode = "USR001";
        _currentUserMock.Setup(u => u.UserCode).Returns(userCode);

        var payments = new List<TblPayment>
        {
            new TblPayment { 
                Code = "P1", 
                OrderCodeNavigation = new TblOrder { UserCode = userCode },
                PaymentDate = DateTime.UtcNow 
            }
        }.BuildMock();

        _paymentRepoMock.Setup(r => r.AsQueryable()).Returns(payments);
        _mapperMock.Setup(m => m.Map<IEnumerable<PaymentDto>>(It.IsAny<List<TblPayment>>()))
            .Returns(new List<PaymentDto> { new PaymentDto { Code = "P1" } });

        // Act
        var result = await _handler.Handle(new GetMyPaymentsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }
}
