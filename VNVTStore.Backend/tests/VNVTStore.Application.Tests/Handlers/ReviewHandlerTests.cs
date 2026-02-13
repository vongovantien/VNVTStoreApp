using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Reviews.Handlers;
using VNVTStore.Application.Reviews.Commands;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Constants;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Reflection;

namespace VNVTStore.Application.Tests.Handlers;

public class ReviewHandlerTests
{
    private readonly Mock<IRepository<TblReview>> _reviewRepoMock;
    private readonly Mock<IRepository<TblOrderItem>> _orderItemRepoMock;
    private readonly Mock<IRepository<TblProduct>> _productRepoMock;
    private readonly Mock<IRepository<TblOrder>> _orderRepository;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDapperContext> _dapperContextMock;
    private readonly Mock<IBaseUrlService> _baseUrlServiceMock;

    private readonly ReviewHandlers _handler;
    private readonly List<TblReview> _reviewsDatabase;

    public ReviewHandlerTests()
    {
        _reviewRepoMock = new Mock<IRepository<TblReview>>();
        _orderItemRepoMock = new Mock<IRepository<TblOrderItem>>();
        _productRepoMock = new Mock<IRepository<TblProduct>>();
        _orderRepository = new Mock<IRepository<TblOrder>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _dapperContextMock = new Mock<IDapperContext>();
        _baseUrlServiceMock = new Mock<IBaseUrlService>();
        _baseUrlServiceMock.Setup(x => x.GetBaseUrl()).Returns("http://test-api.com");

        var orderRepoMock = new Mock<IRepository<TblOrder>>();

        _reviewsDatabase = new List<TblReview>();

        // Setup Repository Mocks
        _reviewRepoMock.Setup(x => x.AddAsync(It.IsAny<TblReview>(), It.IsAny<CancellationToken>()))
            .Callback<TblReview, CancellationToken>((r, _) => _reviewsDatabase.Add(r))
            .Returns(Task.CompletedTask);

        _reviewRepoMock.Setup(x => x.AsQueryable())
            .Returns(CreateMockDbSet(_reviewsDatabase).Object);

        _reviewRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TblReview, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<TblReview, bool>> predicate, CancellationToken _) => 
                _reviewsDatabase.AsQueryable().FirstOrDefault(predicate));

        _productRepoMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string code, CancellationToken _) => 
            {
                var p = TblProduct.Create("Test Product", 100m, null, 10, "CAT1", null, "SUP1");
                SetPrivateProperty(p, "Code", code);
                return p;
            });

        _orderItemRepoMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string code, CancellationToken _) => 
            {
                var item = TblOrderItem.Create("PROD001", "Test", null, 1, 10, null, null);
                SetPrivateProperty(item, "Code", code);
                // Set private OrderCode using reflection since it's private set
                SetPrivateProperty(item, "OrderCode", "ORDER001");
                return item;
            });

        _orderRepository.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string code, CancellationToken _) => 
            {
                var order = TblOrder.Create("USER001", "ADDR001", 10m, 0, 0, null);
                SetPrivateProperty(order, "Code", code);
                return order;
            });

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Setup Mapper
        _mapperMock.Setup(x => x.Map<ReviewDto>(It.IsAny<TblReview>()))
            .Returns((TblReview r) => new ReviewDto 
            { 
                Code = r.Code, 
                UserCode = r.UserCode, 
                ProductCode = r.ProductCode,
                OrderItemCode = r.OrderItemCode,
                Rating = r.Rating,
                Comment = r.Comment
            });

        _mapperMock.Setup(x => x.Map<TblReview>(It.IsAny<CreateReviewDto>()))
            .Returns((CreateReviewDto d) => 
            {
                var r = new TblReview
                {
                    Rating = d.Rating,
                    Comment = d.Comment,
                    UserCode = d.UserCode,
                    ProductCode = d.ProductCode,
                    OrderItemCode = d.OrderItemCode
                };
                return r;
            });

        _handler = new ReviewHandlers(
            _reviewRepoMock.Object,
            _orderItemRepoMock.Object,
            _productRepoMock.Object,
            _orderRepository.Object,
            _currentUserMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _dapperContextMock.Object,
            _baseUrlServiceMock.Object
        );
    }

    [Fact]
    public async Task CreateReview_WithDirectProductCode_ShouldSucceed()
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            UserCode = "USER001",
            ProductCode = "PROD001",
            Rating = 5,
            Comment = "Excellent product!"
        };
        var request = new CreateCommand<CreateReviewDto, ReviewDto>(dto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateReview_WithOrderItem_ShouldLinkToProduct()
    {
        // Arrange
        var orderItem = TblOrderItem.Create("PROD001", "Test Product", null, 1, 10m, null, null);
        SetPrivateProperty(orderItem, nameof(TblOrderItem.Code), "OI001");
        
        var order = TblOrder.Create("USER001", "ADDR001", 10m, 0, 0, null);
        SetPrivateProperty(order, nameof(TblOrder.Code), "ORD001");
        SetPrivateProperty(orderItem, "OrderCode", "ORD001");
        SetPrivateProperty(orderItem, nameof(TblOrderItem.OrderCodeNavigation), order);
        
        _orderItemRepoMock.Setup(x => x.GetByCodeAsync("OI001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

        _orderRepository.Setup(x => x.GetByCodeAsync(order.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var dto = new CreateReviewDto
        {
            UserCode = "USER001",
            OrderItemCode = "OI001",
            Rating = 4
        };
        var request = new CreateCommand<CreateReviewDto, ReviewDto>(dto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateReview_IfAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var review = new TblReview 
        { 
            Code = "R1",
            UserCode = "USER001", 
            ProductCode = "PROD001",
            IsApproved = true
        };
        _reviewsDatabase.Add(review);

        var dto = new CreateReviewDto
        {
            UserCode = "USER001",
            ProductCode = "PROD001",
            Rating = 5
        };
        var request = new CreateCommand<CreateReviewDto, ReviewDto>(dto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(VNVTStore.Application.Common.MessageConstants.ReviewAlreadyExists, result.Error?.Message);
    }

    [Fact]
    public async Task CreateReview_WithWrongUserOrderItem_ShouldReturnForbidden()
    {
        // Arrange
        var orderItem = TblOrderItem.Create("PROD001", "Test Product", null, 1, 10m, null, null);
        SetPrivateProperty(orderItem, nameof(TblOrderItem.Code), "OI001");
        
        var order = TblOrder.Create("OTHER_USER", "ADDR001", 10m, 0, 0, null);
        SetPrivateProperty(order, nameof(TblOrder.Code), "ORD001");
        SetPrivateProperty(orderItem, "OrderCode", "ORD001");
        SetPrivateProperty(orderItem, nameof(TblOrderItem.OrderCodeNavigation), order);

        _orderItemRepoMock.Setup(x => x.GetByCodeAsync("OI001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

        _orderRepository.Setup(x => x.GetByCodeAsync(order.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var dto = new CreateReviewDto
        {
            UserCode = "USER001",
            OrderItemCode = "OI001",
            Rating = 4
        };
        var request = new CreateCommand<CreateReviewDto, ReviewDto>(dto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateReview_WithEmptyStrings_ShouldTreatAsNull()
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            UserCode = "USER001",
            ProductCode = "", // Empty string from FE
            OrderItemCode = "", // Empty string from FE
            Rating = 5,
            Comment = "Excellent"
        };
        var request = new CreateCommand<CreateReviewDto, ReviewDto>(dto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var capturedReview = _reviewsDatabase.Last();
        Assert.Null(capturedReview.ProductCode);
        Assert.Null(capturedReview.OrderItemCode);
    }

    [Fact]
    public async Task ReplyReview_ShouldInheritProductCodeFromParent()
    {
        // Arrange
        var parentReview = new TblReview
        {
            Code = "REV001",
            UserCode = "USER001",
            ProductCode = "PROD001",
            Comment = "Parent comment"
        };
        _reviewRepoMock.Setup(x => x.GetByCodeAsync("REV001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentReview);
        
        _currentUserMock.Setup(x => x.UserCode).Returns("ADMIN001");

        var command = new ReplyReviewCommand("REV001", "Reply comment");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var reply = _reviewsDatabase.Last();
        Assert.Equal("PROD001", reply.ProductCode);
        Assert.Equal("REV001", reply.ParentCode);
    }

    private void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();
        var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        if (prop != null)
        {
            prop.SetValue(obj, value);
            return;
        }

        var fieldName = $"<{propertyName}>k__BackingField";
        var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
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
