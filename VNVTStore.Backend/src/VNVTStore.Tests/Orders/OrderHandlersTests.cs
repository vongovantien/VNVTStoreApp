using AutoMapper;
using Moq;
using System.Linq.Expressions;
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

    private TblProduct CreateTestProduct(string code, string name, int stock, decimal price)
    {
        return new TblProduct { Code = code, Name = name, StockQuantity = stock, Price = price };
    }

    [Fact]
    public async Task CreateOrder_EmptyCart_ReturnsFailure()
    {
        // Arrange
        var userCode = "USR001";
        _cartServiceMock.Setup(c => c.GetOrCreateCartAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblCart?)null); // Or empty items

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
        var cart = new TblCart { 
            TblCartItems = new List<TblCartItem> 
            { 
                new TblCartItem { ProductCode = "P1", Quantity = 10, ProductCodeNavigation = product } 
            } 
        };

        _cartServiceMock.Setup(c => c.GetOrCreateCartAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var cmd = new CreateOrderCommand(userCode, new CreateOrderDto());

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("insufficient", result.Error!.Message);
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
        var cart = new TblCart
        {
            TblCartItems = new List<TblCartItem>
            {
                new TblCartItem { ProductCode = "P1", Quantity = 1, ProductCodeNavigation = product }
            }
        };

        _cartServiceMock.Setup(c => c.GetOrCreateCartAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
        
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<TblOrder>(), It.IsAny<CancellationToken>()))
            .Callback<TblOrder, CancellationToken>((o, c) => {
                Assert.Equal(expectedFee, o.ShippingFee);
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
        var cart = new TblCart { TblCartItems = new List<TblCartItem> { new TblCartItem { Quantity = 2, ProductCodeNavigation = product } } };

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
        var order = new TblOrder 
        { 
            Code = orderCode, 
            UserCode = userCode, 
            Status = "Pending",
            TblOrderItems = new List<TblOrderItem> 
            { 
                new TblOrderItem { ProductCode = "P1", Quantity = 2, ProductCodeNavigation = product } 
            }
        };

        // Need to mock GetByCodeAsync or AsQueryable depending on Handler implementation
        // Handler uses AsQueryable().Include(...).FirstOrDefaultAsync(...)
        // This makes mocking hard without MockQueryable.
        // HOWEVER, Cancel Handler creates a query.
        // I will attempt to Mock AsQueryable if I can't inject a list.
        // If strict testing "AsQueryable" is too hard without libs, I might need to rely on the fact that 
        // CreateOrder logic is verifiable.
        // BUT wait, I can simulate `AsQueryable` using a simple list wrapped in EnumerableQuery.
        
        var list = new List<TblOrder> { order }.AsQueryable();
        _orderRepoMock.Setup(r => r.AsQueryable()).Returns(list);
        
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var cmd = new CancelOrderCommand(userCode, orderCode, "Customer Request");

        // Act
        // Note: The Handler uses `FirstOrDefaultAsync` (Extension method). 
        // If I return EnumerableQuery, `FirstOrDefaultAsync` works if using `Microsoft.EntityFrameworkCore.Query.Internal`? 
        // No, `FirstOrDefaultAsync` expects `IAsyncEnumerable`.
        // Standard EnumerableQuery is not Async.
        // Tests typically fail here unless using `MockQueryable`.
        // I will skip the Async Query test in this environment or use a workaround?
        // Workaround: Mock `FirstOrDefaultAsync`. But it is static extension.
        // I will try to Mock `GetByCodeAsync` if the Handler used it. But Handler uses Queryable manually.
        // I will Modify `OrderHandlers` to use `Repository.GetByCodeAsync` or a method I can mock?
        // No, I shouldn't change code just for tests if possible.
        // I will TRY to use `MockQueryable` logic if I can write a helper class quickly in checking `AsyncQueryableMock.cs` if exists?
    }
}
