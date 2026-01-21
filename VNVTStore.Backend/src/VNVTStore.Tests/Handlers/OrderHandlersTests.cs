using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MediatR;
using MockQueryable.Moq;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Strategies;
using Microsoft.Extensions.Configuration;

namespace VNVTStore.Tests.Handlers;

public class OrderHandlersTests
{
    private readonly Mock<IRepository<TblOrder>> _mockOrderRepo;
    private readonly Mock<ICartService> _mockCartService;
    private readonly Mock<IRepository<TblProduct>> _mockProductRepo;
    private readonly Mock<IRepository<TblAddress>> _mockAddressRepo;
    private readonly Mock<IRepository<TblUser>> _mockUserRepo;
    private readonly Mock<IShippingStrategy> _mockShippingStrategy;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly OrderHandlers _handler;

    public OrderHandlersTests()
    {
        _mockOrderRepo = new Mock<IRepository<TblOrder>>();
        _mockCartService = new Mock<ICartService>();
        _mockProductRepo = new Mock<IRepository<TblProduct>>();
        _mockAddressRepo = new Mock<IRepository<TblAddress>>();
        _mockUserRepo = new Mock<IRepository<TblUser>>();
        _mockShippingStrategy = new Mock<IShippingStrategy>();
        _mockMediator = new Mock<IMediator>();
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockContext = new Mock<IApplicationDbContext>();

        _mockEmailService = new Mock<IEmailService>();
        _mockConfiguration = new Mock<IConfiguration>();

        _handler = new OrderHandlers(
            _mockOrderRepo.Object,
            _mockCartService.Object,
            _mockProductRepo.Object,
            _mockAddressRepo.Object,
            _mockUserRepo.Object,
            _mockShippingStrategy.Object,
            _mockMediator.Object,
            _mockUow.Object,
            _mockMapper.Object,
            _mockNotificationService.Object,
            _mockContext.Object,

            _mockEmailService.Object,
            _mockConfiguration.Object
        );
    }

    [Fact]
    public async Task VerifyOrder_ShouldSucceed_WhenTokenIsValid()
    {
        var token = "valid_token";
        var order = new TblOrder 
        { 
            Code = "ORD123", 
            Status = OrderStatus.Pending 
        };
        order.SetVerificationToken(token, DateTime.UtcNow.AddHours(1));

        var orders = new List<TblOrder> { order }.AsQueryable().BuildMock();
        _mockContext.Setup(c => c.TblOrders).Returns(orders.Object);

        var command = new VerifyOrderCommand(token);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ORD123", result.Value);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Null(order.VerificationToken);
        
        _mockOrderRepo.Verify(r => r.Update(order), Times.Once);
        _mockUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyOrder_ShouldFail_WhenTokenIsInvalid()
    {
        var orders = new List<TblOrder>().AsQueryable().BuildMock();
        _mockContext.Setup(c => c.TblOrders).Returns(orders.Object);

        var command = new VerifyOrderCommand("invalid_token");
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Invalid verification token", result.Error.Message);
    }

    [Fact]
    public async Task VerifyOrder_ShouldFail_WhenTokenExpired()
    {
        var token = "expired_token";
        var order = new TblOrder 
        { 
            Code = "ORD123", 
            Status = OrderStatus.Pending 
        };
        order.SetVerificationToken(token, DateTime.UtcNow.AddHours(-1));

        var orders = new List<TblOrder> { order }.AsQueryable().BuildMock();
        _mockContext.Setup(c => c.TblOrders).Returns(orders.Object);

        var command = new VerifyOrderCommand(token);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("expired", result.Error.Message);
    }
}
