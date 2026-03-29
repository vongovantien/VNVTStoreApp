using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using VNVTStore.Application.Carts.Commands;
using VNVTStore.Application.Carts.Handlers;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Tests.Handlers;

public class CartHandlersTests
{
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly Mock<IRepository<TblProduct>> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CartHandlers>> _loggerMock;
    private readonly CartHandlers _handler;

    public CartHandlersTests()
    {
        _cartServiceMock = new Mock<ICartService>();
        _productRepoMock = new Mock<IRepository<TblProduct>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CartHandlers>>();

        _handler = new CartHandlers(
            _cartServiceMock.Object,
            _productRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_AddToCart_ProductNotFound_ShouldReturnFailure()
    {
        var product = TblProduct.Create("Product 1", 100, null, 100, "CAT001", null, null);
        _productRepoMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblProduct)null);

        var command = new AddToCartCommand("USER001", "NONEXISTENT", 1, null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }

    [Fact]
    public async Task Handle_AddToCart_InsufficientStock_ShouldReturnFailure()
    {
        // Arrange
        var product = TblProduct.Create("PROD001", 100, null, 5, "CAT001", null, null);
        _productRepoMock.Setup(x => x.GetByCodeAsync("PROD001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var cart = TblCart.Create("USER001");
        _cartServiceMock.Setup(x => x.GetOrCreateCartAsync("USER001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new AddToCartCommand("USER001", "PROD001", 10, null, null); // Requesting 10 but only 5 available

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error?.Code);
    }

    [Fact]
    public async Task Handle_AddToCart_ShouldSucceed()
    {
        // Arrange
        var product = TblProduct.Create("PROD001", 100, null, 100, "CAT001", null, null);
        _productRepoMock.Setup(x => x.GetByCodeAsync("PROD001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var cart = TblCart.Create("USER001");
        _cartServiceMock.Setup(x => x.GetOrCreateCartAsync("USER001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _mapperMock.Setup(x => x.Map<CartDto>(cart)).Returns(new CartDto { UserCode = "USER001" });

        var command = new AddToCartCommand("USER001", "PROD001", 2, "L", "Black");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(cart.TblCartItems);
        Assert.Equal(2, cart.TblCartItems.First().Quantity);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_UpdateCartItem_NotFound_ShouldReturnFailure()
    {
        // Arrange
        var cart = TblCart.Create("USER001");
        _cartServiceMock.Setup(x => x.GetOrCreateCartAsync("USER001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new UpdateCartItemCommand("USER001", "ITEM001", 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }
}
