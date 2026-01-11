using AutoMapper;
using Moq;
using System.Reflection;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Tests.Common;
using Xunit;

namespace VNVTStore.Tests.Orders;

public class OrderHandlersTests
{
    private readonly Mock<IRepository<TblOrder>> _orderRepoMock;
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly Mock<IRepository<TblProduct>> _productRepoMock;
    private readonly Mock<IRepository<TblAddress>> _addressRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly OrderHandlers _handler;

    public OrderHandlersTests()
    {
        _orderRepoMock = new Mock<IRepository<TblOrder>>();
        _cartServiceMock = new Mock<ICartService>();
        _productRepoMock = new Mock<IRepository<TblProduct>>();
        _addressRepoMock = new Mock<IRepository<TblAddress>>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _handler = new OrderHandlers(
            _orderRepoMock.Object,
            _cartServiceMock.Object,
            _productRepoMock.Object,
            _addressRepoMock.Object,
            _uowMock.Object,
            _mapperMock.Object
        );
    }

    private void SetPrivate<T>(object obj, string propName, T value)
    {
        var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null)
        {
             prop.SetValue(obj, value);
        }
    }

    private TblProduct CreateTestProduct(string code, string name, int stock, decimal price)
    {
        // Factory creates with random code, so we override it
        var p = TblProduct.Create(name, price, stock, "CAT1", "SKU1");
        SetPrivate(p, "Code", code);
        return p;
    }

    [Fact]
    public async Task CreateOrder_EmptyCart_ReturnsFailure()
    {
        // Arrange
        var userCode = "USR001";
        _cartServiceMock.Setup(c => c.GetOrCreateCartAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblCart?)null); 

        var cmd = new CreateOrderCommand(userCode, new CreateOrderDto());

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(MessageConstants.CartEmpty, result.Error!.Message);
    }

    [Fact]
    public async Task CreateOrder_InsufficientStock_ReturnsFailure()
    {
        // Arrange
        var userCode = "USR001";
        var product = CreateTestProduct("P1", "Phone", 5, 1000);
        
        var cart = TblCart.Create(userCode);
        SetPrivate(cart, "Code", "CART1");
        
        var cartItem = TblCartItem.Create("CART1", "P1", 10, null, null);
        SetPrivate(cartItem, "ProductCodeNavigation", product);
        
        // Use reflection or internal method if Add is not adding directly to collection exposed?
        // TblCart has public TblCartItems collection (ICollection).
        cart.TblCartItems.Add(cartItem);

        _cartServiceMock.Setup(c => c.GetOrCreateCartAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var cmd = new CreateOrderCommand(userCode, new CreateOrderDto());

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        // Note: The Handler iterates cart items and checks stock.
        // It should return failure.
        Assert.Contains("insufficient", result.Error!.Message, StringComparison.OrdinalIgnoreCase); 
    }

    [Theory]
    [InlineData(500000, 30000)] // < 1M -> 30k fee
    [InlineData(1000000, 0)]    // = 1M -> Free
    [InlineData(2000000, 0)]    // > 1M -> Free
    public async Task CreateOrder_CalculatesShippingFee_Correctly(decimal itemPrice, decimal expectedFee)
    {
        // Arrange
        var userCode = "USR001";
        var product = CreateTestProduct("P1", "Item", 10, itemPrice);

        var cart = TblCart.Create(userCode);
        SetPrivate(cart, "Code", "CART1");
        var cartItem = TblCartItem.Create("CART1", "P1", 1, null, null);
        SetPrivate(cartItem, "ProductCodeNavigation", product);
        cart.TblCartItems.Add(cartItem);

        _cartServiceMock.Setup(c => c.GetOrCreateCartAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
        
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<TblOrder>(), It.IsAny<CancellationToken>()))
            .Callback<TblOrder, CancellationToken>((o, c) => {
                Assert.Equal(expectedFee, o.ShippingFee);
                // Check Final Amount
                Assert.Equal(itemPrice + expectedFee, o.FinalAmount);
            })
            .Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<TblOrder>())).Returns(new OrderDto());

        var cmd = new CreateOrderCommand(userCode, new CreateOrderDto 
        { 
            Address = "123 Street", City = "HCM", PaymentMethod = "COD" 
        });

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _cartServiceMock.Verify(c => c.ClearCartAsync(userCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ValidOrder_DeductsStock()
    {
        // Arrange
        var userCode = "USR001";
        var product = CreateTestProduct("P1", "Item", 10, 100); // Stock 10
        
        var cart = TblCart.Create(userCode);
        SetPrivate(cart, "Code", "CART1");
        var cartItem = TblCartItem.Create("CART1", "P1", 2, null, null);
        SetPrivate(cartItem, "ProductCodeNavigation", product);
        cart.TblCartItems.Add(cartItem);

        _cartServiceMock.Setup(c => c.GetOrCreateCartAsync(userCode, It.IsAny<CancellationToken>()))
             .ReturnsAsync(cart);
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<TblOrder>())).Returns(new OrderDto());

        var cmd = new CreateOrderCommand(userCode, new CreateOrderDto { Address = "Addr", City = "City", PaymentMethod = "COD" });

        // Act
        await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.Equal(8, product.StockQuantity); // 10 - 2
        _productRepoMock.Verify(p => p.Update(product), Times.Once);
    }

    [Fact]
    public async Task CancelOrder_RestoresStock()
    {
        // Arrange
        var orderCode = "ORD001";
        var userCode = "USR001";
        var product = CreateTestProduct("P1", "Item", 8, 100); // Current Stock 8
        
        // TblOrder.Create requires args.
        var order = TblOrder.Create(userCode, "ADDR1", 200, 0, 0, null);
        SetPrivate(order, "Code", orderCode);
        
        var orderItem = new TblOrderItem 
        { 
            Code = "OI1", // manual
            ProductCode = "P1", 
            Quantity = 2 
        };
        SetPrivate(orderItem, "ProductCodeNavigation", product);
        order.AddOrderItem(orderItem);

        // Mock AsQueryable to return this order
        var list = new List<TblOrder> { order };
        var mockSet = new TestAsyncEnumerable<TblOrder>(list);

        _orderRepoMock.Setup(r => r.AsQueryable()).Returns(mockSet);
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var cmd = new CancelOrderCommand(userCode, orderCode, "Customer Request");

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(10, product.StockQuantity); // 8 + 2
        Assert.Equal("Cancelled", order.Status);
    }
}
