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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;

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

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();
        var dbSetMock = new Mock<DbSet<T>>();

        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        
        dbSetMock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        return dbSetMock;
    }

    [Fact]
    public async Task Handle_AddToCart_ProductNotFound_ShouldReturnFailure()
    {
        // Arrange
        var products = new List<TblProduct>();
        _productRepoMock.Setup(x => x.AsQueryable())
            .Returns(CreateMockDbSet(products).Object);

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
        var product = TblProduct.Create("Product 1", 100, null, 5, "CAT001", null, null);
        typeof(TblProduct).GetProperty("Code")?.SetValue(product, "PROD001");

        var products = new List<TblProduct> { product };
        _productRepoMock.Setup(x => x.AsQueryable())
            .Returns(CreateMockDbSet(products).Object);

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
        var product = TblProduct.Create("Product 1", 100, null, 100, "CAT001", null, null);
        typeof(TblProduct).GetProperty("Code")?.SetValue(product, "PROD001");
        
        var products = new List<TblProduct> { product };
        _productRepoMock.Setup(x => x.AsQueryable())
            .Returns(CreateMockDbSet(products).Object);

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

    // EF Mock Helpers
    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object? Execute(Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult);
            var expectedResultType = resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>) 
                ? resultType.GetGenericArguments()[0] 
                : resultType;
                
            var executionResult = _inner.Execute(expression);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(expectedResultType).Invoke(null, new[] { executionResult })!;
        }
    }
    
    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(Expression expression) : base(expression) { }
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
