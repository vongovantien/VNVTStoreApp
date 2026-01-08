using AutoMapper;
using Moq;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Application.Orders.Handlers;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Tests.Extensions;
using Xunit;

namespace VNVTStore.Tests.Orders;

public class OrderHandlersTests
{
    private readonly Mock<IRepository<TblOrder>> _orderRepoMock;
    private readonly Mock<IRepository<TblCart>> _cartRepoMock;
    private readonly Mock<IRepository<TblProduct>> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly OrderHandlers _handler;

    public OrderHandlersTests()
    {
        _orderRepoMock = new Mock<IRepository<TblOrder>>();
        _cartRepoMock = new Mock<IRepository<TblCart>>();
        _productRepoMock = new Mock<IRepository<TblProduct>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _handler = new OrderHandlers(
            _orderRepoMock.Object,
            _cartRepoMock.Object,
            _productRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task GetOrderById_OrderNotFound_ReturnsFailure()
    {
        // Arrange
        var orderCode = "ORD999";
        var orders = new List<TblOrder>().BuildMock();
        _orderRepoMock.Setup(r => r.AsQueryable()).Returns(orders);

        // Act
        var result = await _handler.Handle(new GetOrderByIdQuery(orderCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateOrderStatus_OrderNotFound_ReturnsFailure()
    {
        // Arrange
        var orderCode = "ORD999";
        _orderRepoMock.Setup(r => r.GetByCodeAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblOrder?)null);

        // Act
        var result = await _handler.Handle(new UpdateOrderStatusCommand(orderCode, "Shipped"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task UpdateOrderStatus_Success_ReturnsUpdatedOrder()
    {
        // Arrange
        var orderCode = "ORD001";
        var order = new TblOrder 
        { 
            Code = orderCode, 
            UserCode = "USR001", 
            Status = "Pending",
            TotalAmount = 500,
            FinalAmount = 500
        };
        var orderDto = new OrderDto { Code = orderCode, Status = "Shipped" };

        _orderRepoMock.Setup(r => r.GetByCodeAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<TblOrder>())).Returns(orderDto);

        // Act
        var result = await _handler.Handle(new UpdateOrderStatusCommand(orderCode, "Shipped"), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Shipped", order.Status);
    }

    [Fact]
    public async Task CancelOrder_OrderNotFound_ReturnsFailure()
    {
        // Arrange
        var userCode = "USR001";
        var orderCode = "ORD999";

        var orders = new List<TblOrder>().BuildMock();
        _orderRepoMock.Setup(r => r.AsQueryable()).Returns(orders);

        // Act
        var result = await _handler.Handle(new CancelOrderCommand(userCode, orderCode, "Changed mind"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelOrder_NotOwner_ReturnsForbidden()
    {
        // Arrange
        var userCode = "USR001";
        var orderCode = "ORD001";
        var order = new TblOrder 
        { 
            Code = orderCode, 
            UserCode = "USR002", // Different user
            Status = "Pending",
            TblOrderItems = new List<TblOrderItem>()
        };

        var orders = new List<TblOrder> { order }.BuildMock();
        _orderRepoMock.Setup(r => r.AsQueryable()).Returns(orders);

        // Act
        var result = await _handler.Handle(new CancelOrderCommand(userCode, orderCode, "Changed mind"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Forbidden", result.Error!.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelOrder_NotPending_ReturnsValidationError()
    {
        // Arrange
        var userCode = "USR001";
        var orderCode = "ORD001";
        var order = new TblOrder 
        { 
            Code = orderCode, 
            UserCode = userCode,
            Status = "Shipped", // Not pending
            TblOrderItems = new List<TblOrderItem>()
        };
        
        var orders = new List<TblOrder> { order }.BuildMock();
        _orderRepoMock.Setup(r => r.AsQueryable()).Returns(orders);

        // Act
        var result = await _handler.Handle(new CancelOrderCommand(userCode, orderCode, "Changed mind"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("pending", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

}
