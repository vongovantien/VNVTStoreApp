using AutoMapper;
using Moq;
using VNVTStore.Application.Carts.Commands;
using VNVTStore.Application.Carts.Queries;
using VNVTStore.Application.Carts.Handlers;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Tests.Extensions;
using Xunit;

namespace VNVTStore.Tests.Carts;

public class CartHandlersTests
{
    private readonly Mock<IRepository<TblCart>> _cartRepoMock;
    private readonly Mock<IRepository<TblProduct>> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CartHandlers _handler;

    public CartHandlersTests()
    {
        _cartRepoMock = new Mock<IRepository<TblCart>>();
        _productRepoMock = new Mock<IRepository<TblProduct>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _handler = new CartHandlers(
            _cartRepoMock.Object,
            _productRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task GetMyCart_WithExistingCart_ReturnsCartDto()
    {
        // Arrange
        var userCode = "USR001";
        var cart = new TblCart 
        { 
            Code = "CRT001", 
            UserCode = userCode,
            TblCartItems = new List<TblCartItem>()
        };
        var cartDto = new CartDto { Code = "CRT001", UserCode = userCode };
        
        var carts = new List<TblCart> { cart }.BuildMock();
        _cartRepoMock.Setup(r => r.AsQueryable()).Returns(carts);
        
        _mapperMock.Setup(m => m.Map<CartDto>(It.IsAny<TblCart>())).Returns(cartDto);

        // Act
        var result = await _handler.Handle(new GetMyCartQuery(userCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetMyCart_WithNoCart_CreatesNew()
    {
        // Arrange
        var userCode = "USR002";
        
        var carts = new List<TblCart>().BuildMock();
        _cartRepoMock.Setup(r => r.AsQueryable()).Returns(carts); // Empty list

        _cartRepoMock.Setup(r => r.AddAsync(It.IsAny<TblCart>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _mapperMock.Setup(m => m.Map<CartDto>(It.IsAny<TblCart>()))
            .Returns((TblCart c) => new CartDto { Code = c.Code, UserCode = c.UserCode });

        // Act
        var result = await _handler.Handle(new GetMyCartQuery(userCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _cartRepoMock.Verify(r => r.AddAsync(It.IsAny<TblCart>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddToCart_ProductNotFound_ReturnsFailure()
    {
        // Arrange
        var userCode = "USR001";
        var productCode = "PRD999";
        
        _productRepoMock.Setup(r => r.GetByCodeAsync(productCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblProduct?)null);

        // Act
        var result = await _handler.Handle(new AddToCartCommand(userCode, productCode, 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddToCart_InsufficientStock_ReturnsFailure()
    {
        // Arrange
        var userCode = "USR001";
        var productCode = "PRD001";
        var product = new TblProduct { Code = productCode, Name = "Test Product", StockQuantity = 2, Price = 100 };
        
        _productRepoMock.Setup(r => r.GetByCodeAsync(productCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(new AddToCartCommand(userCode, productCode, 5), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("stock", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ClearCart_Success_ReturnsTrue()
    {
        // Arrange
        var userCode = "USR001";
        var cart = new TblCart 
        { 
            Code = "CRT001", 
            UserCode = userCode,
            TblCartItems = new List<TblCartItem>
            {
                new TblCartItem { Code = "CRI001", ProductCode = "PRD001", Quantity = 2 }
            }
        };

        var carts = new List<TblCart> { cart }.BuildMock();
        _cartRepoMock.Setup(r => r.AsQueryable()).Returns(carts);
        
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new ClearCartCommand(userCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }
    
    [Fact]
    public async Task RemoveFromCart_ItemNotInCart_ReturnsSuccess() // If not in cart, still success logic but does nothing? Code logic: returns Success regardless
    {
        // Arrange
        var userCode = "USR001";
        var productCode = "PRD999";
        var cart = new TblCart 
        { 
            Code = "CRT001", 
            UserCode = userCode,
            TblCartItems = new List<TblCartItem>()
        };

        var carts = new List<TblCart> { cart }.BuildMock();
        _cartRepoMock.Setup(r => r.AsQueryable()).Returns(carts);
        
        _mapperMock.Setup(m => m.Map<CartDto>(It.IsAny<TblCart>())).Returns(new CartDto());

        // Act
        var result = await _handler.Handle(new RemoveFromCartCommand(userCode, productCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

}
