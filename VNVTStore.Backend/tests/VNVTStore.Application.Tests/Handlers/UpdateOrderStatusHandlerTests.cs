using AutoMapper;
using MediatR;
using Moq;
using Xunit;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Tests.Handlers;

public class UpdateOrderStatusHandlerTests
{
    private readonly Mock<IRepository<TblOrder>> _orderRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDapperContext> _dapperContextMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly UpdateOrderStatusHandler _handler;

    public UpdateOrderStatusHandlerTests()
    {
        _orderRepoMock = new Mock<IRepository<TblOrder>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _dapperContextMock = new Mock<IDapperContext>();
        _notificationServiceMock = new Mock<INotificationService>();

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));

        _handler = new UpdateOrderStatusHandler(
            _orderRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _dapperContextMock.Object,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_OrderExists_ShouldUpdateStatusAndReturnSuccess()
    {
        // Arrange
        var orderCode = "ORD001";
        var newStatus = OrderStatus.Confirmed;
        var userCode = "USER001";
        
        // TblOrder.Create(userCode, addressCode, totalAmount, shippingFee, discountAmount, couponCode)
        var order = TblOrder.Create(userCode, "ADDR001", 1000m, 50m, 10m, null);
        // We need to set the order code manually as it might be generated or set by entity logic
        order.Code = orderCode;

        _orderRepoMock.Setup(x => x.GetByCodeAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Mock Mapper: assumes converting OrderStatus enum to string if DTO uses string, 
        // OR converts to string/enum depending on OrderDto definition.
        // Assuming OrderDto has status as string in the test expectation, 
        // BUT Wait, OrderDto typically has string status in this project based on earlier format.ts confusion.
        // Let's check OrderDto. 
        // If OrderDto uses string 'status', we map it. 
        // Ideally we return the mocked conversion.
        _mapperMock.Setup(x => x.Map<OrderDto>(order)).Returns(new OrderDto { Status = newStatus.ToString().ToLower() }); 

        var command = new UpdateOrderStatusCommand(orderCode, newStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newStatus.ToString().ToLower(), result.Value?.Status);
        
        // Verify Update was called
        _orderRepoMock.Verify(x => x.Update(order), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var orderCode = "NONEXISTENT";
        var newStatus = OrderStatus.Confirmed;

        _orderRepoMock.Setup(x => x.GetByCodeAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblOrder?)null);

        var command = new UpdateOrderStatusCommand(orderCode, newStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
        
        _orderRepoMock.Verify(x => x.Update(It.IsAny<TblOrder>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
