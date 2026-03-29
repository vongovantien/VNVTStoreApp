using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Strategies;

namespace VNVTStore.Application.Tests.Handlers;

public class OrderHandlersTests
{
    private readonly Mock<IRepository<TblOrder>> _orderRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDapperContext> _dapperContextMock;
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly Mock<IRepository<TblProduct>> _productRepoMock;
    private readonly Mock<IRepository<TblAddress>> _addressRepoMock;
    private readonly Mock<IRepository<TblUser>> _userRepoMock;
    private readonly Mock<IShippingStrategy> _shippingStrategyMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILoyaltyService> _loyaltyServiceMock;
    private readonly Mock<ISecretConfigurationService> _secretConfigMock;
    private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private readonly CreateOrderHandler _handler;

    public OrderHandlersTests()
    {
        _orderRepoMock = new Mock<IRepository<TblOrder>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _dapperContextMock = new Mock<IDapperContext>();
        _cartServiceMock = new Mock<ICartService>();
        _productRepoMock = new Mock<IRepository<TblProduct>>();
        _addressRepoMock = new Mock<IRepository<TblAddress>>();
        _userRepoMock = new Mock<IRepository<TblUser>>();
        _shippingStrategyMock = new Mock<IShippingStrategy>();
        _mediatorMock = new Mock<IMediator>();
        _notificationServiceMock = new Mock<INotificationService>();
        _contextMock = new Mock<IApplicationDbContext>();
        _emailServiceMock = new Mock<IEmailService>();
        _configurationMock = new Mock<IConfiguration>();
        _loyaltyServiceMock = new Mock<ILoyaltyService>();
        _secretConfigMock = new Mock<ISecretConfigurationService>();
        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();

        // Setup UnitOfWork
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
        _unitOfWorkMock.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _handler = new CreateOrderHandler(
            _orderRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _dapperContextMock.Object,
            _cartServiceMock.Object,
            _productRepoMock.Object,
            _addressRepoMock.Object,
            _userRepoMock.Object,
            _shippingStrategyMock.Object,
            _mediatorMock.Object,
            _notificationServiceMock.Object,
            _contextMock.Object,
            _emailServiceMock.Object,
            _configurationMock.Object,
            _loyaltyServiceMock.Object,
            _secretConfigMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CreateOrder_CartEmpty_ShouldReturnFailure()
    {
        // Arrange
        var userCode = "USER001";
        _cartServiceMock.Setup(x => x.GetOrCreateCartAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TblCart.Create(userCode)); // Empty cart

        var command = new CreateOrderCommand(userCode, new CreateOrderDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error?.Code);
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateOrder_InsufficientStock_ShouldReturnFailure()
    {
        // Arrange
        var userCode = "USER001";
        var product = TblProduct.Create("Bad Product", 100, null, 2, "CAT001", null, null);
        product.Code = "PROD001"; 

        var cart = TblCart.Create(userCode);
        cart.AddItem(product.Code, 5, null, null, 100); 

        _cartServiceMock.Setup(x => x.GetOrCreateCartAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var cartItem = cart.TblCartItems.First();
        var productNavigationField = typeof(TblCartItem).GetProperty("ProductCodeNavigation");
        productNavigationField?.SetValue(cartItem, product);

        _productRepoMock.Setup(x => x.ReloadAsync(product, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Mock TblFiles with Async support
        var mockFiles = new List<TblFile>().AsQueryable();
        var dbSetMock = CreateMockDbSet(mockFiles);
        _contextMock.Setup(x => x.TblFiles).Returns(dbSetMock.Object);

        var command = new CreateOrderCommand(userCode, new CreateOrderDto { AddressCode = "ADDR001" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error?.Code);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(data.Provider));

        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        return mockSet;
    }

    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object? Execute(System.Linq.Expressions.Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) => _inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, new[] { Execute(expression) })!;
        }
    }

    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public T Current => _inner.Current;
        public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    }
}
