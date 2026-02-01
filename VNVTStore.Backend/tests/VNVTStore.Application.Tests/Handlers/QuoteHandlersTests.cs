using Moq;
using AutoMapper;
using Xunit;
using System.Collections.Generic;
using VNVTStore.Application.Quotes.Handlers;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common;
using VNVTStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace VNVTStore.Application.Tests.Handlers;

public class QuoteHandlersTests
{
    private readonly Mock<IRepository<TblQuote>> _quoteRepoMock;
    private readonly Mock<IRepository<TblProduct>> _productRepoMock;
    private readonly Mock<IRepository<TblUser>> _userRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly QuoteHandlers _handler;

    public QuoteHandlersTests()
    {
        _quoteRepoMock = new Mock<IRepository<TblQuote>>();
        _productRepoMock = new Mock<IRepository<TblProduct>>();
        _userRepoMock = new Mock<IRepository<TblUser>>();
        _emailServiceMock = new Mock<IEmailService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _handler = new QuoteHandlers(
            _quoteRepoMock.Object,
            _productRepoMock.Object,
            _userRepoMock.Object,
            _emailServiceMock.Object,
            _notificationServiceMock.Object,
            _currentUserMock.Object,
            _uowMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_CreateQuote_ShouldSendEmailToAdmins()
    {
        // Arrange
        var request = new CreateCommand<CreateQuoteDto, QuoteDto>(new CreateQuoteDto
        {
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            Items = new List<CreateQuoteItemDto> { new CreateQuoteItemDto { ProductCode = "P1", Quantity = 1 } }
        });

        _currentUserMock.Setup(x => x.UserCode).Returns("U1");
        _productRepoMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TblProduct.Create("Test Product", 10m, null, 100, null, null, null));

        var admins = new List<TblUser>
        {
            TblUser.Create("admin", "admin@example.com", "hash", "Admin User", UserRole.Admin)
        };

        _userRepoMock.Setup(x => x.AsQueryable()).Returns(CreateMockDbSet(admins).Object);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            "admin@example.com",
            It.Is<string>(s => s.Contains("[New Quote Request]")),
            It.IsAny<string>(),
            true), Times.Once);

        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _quoteRepoMock.Verify(x => x.AddAsync(It.IsAny<TblQuote>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateQuote_WhenNoAdmins_ShouldNotSendEmail()
    {
        // Arrange
        var request = new CreateCommand<CreateQuoteDto, QuoteDto>(new CreateQuoteDto
        {
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            Items = new List<CreateQuoteItemDto> { new CreateQuoteItemDto { ProductCode = "P1", Quantity = 1 } }
        });

        _currentUserMock.Setup(x => x.UserCode).Returns("U1");
        _productRepoMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TblProduct.Create("Test Product", 10m, null, 100, null, null, null));

        var noAdmins = new List<TblUser>();

        _userRepoMock.Setup(x => x.AsQueryable()).Returns(CreateMockDbSet(noAdmins).Object);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()), Times.Never);
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
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executeMethod = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) });
            var result = executeMethod!.MakeGenericMethod(resultType).Invoke(_inner, new object[] { expression });
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, new[] { result })!;
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
