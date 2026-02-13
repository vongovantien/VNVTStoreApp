using AutoMapper;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Infrastructure.Services;
using Xunit;

namespace VNVTStore.Application.Tests.Services;

public class CouponServiceTests
{
    private readonly Mock<IRepository<TblCoupon>> _couponRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CouponService _couponService;

    public CouponServiceTests()
    {
        _couponRepoMock = new Mock<IRepository<TblCoupon>>();
        _mapperMock = new Mock<IMapper>();
        _couponService = new CouponService(_couponRepoMock.Object, _mapperMock.Object);
    }

    private void SetupMockRepo(List<TblCoupon> coupons)
    {
        var mockDbSet = CreateMockDbSet(coupons);
        _couponRepoMock.Setup(r => r.AsQueryable()).Returns(mockDbSet.Object);
        _mapperMock.Setup(m => m.Map<CouponDto>(It.IsAny<TblCoupon>()))
            .Returns((TblCoupon c) => new CouponDto { Code = c.Code });
    }

    [Fact]
    public async Task ValidateCoupon_ShouldFail_WhenCouponNotFound()
    {
        // Arrange
        SetupMockRepo(new List<TblCoupon>());

        // Act
        var result = await _couponService.ValidateCouponAsync("INVALID", 100);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task ValidateCoupon_ShouldFail_WhenPromotionExpired()
    {
        // Arrange
        var coupon = new TblCoupon 
        { 
            Code = "EXPIRED", 
            PromotionCodeNavigation = new TblPromotion 
            { 
                IsActive = true,
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(-1) 
            } 
        };
        SetupMockRepo(new List<TblCoupon> { coupon });

        // Act
        var result = await _couponService.ValidateCouponAsync("EXPIRED", 100);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Validation"); // Or specific expired error code if defined
    }

    [Fact]
    public async Task ValidateCoupon_ShouldFail_WhenMinOrderAmountNotMet()
    {
        // Arrange
        var coupon = new TblCoupon 
        { 
            Code = "MIN_ORDER", 
            PromotionCodeNavigation = new TblPromotion 
            { 
                IsActive = true,
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1),
                MinOrderAmount = 500000
            } 
        };
        SetupMockRepo(new List<TblCoupon> { coupon });

        // Act
        var result = await _couponService.ValidateCouponAsync("MIN_ORDER", 499000);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Minimum order");
    }

    [Fact]
    public async Task ValidateCoupon_ShouldSucceed_WhenValid()
    {
        // Arrange
        var coupon = new TblCoupon 
        { 
            Code = "VALID", 
            PromotionCodeNavigation = new TblPromotion 
            { 
                IsActive = true,
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1),
                MinOrderAmount = 100000
            } 
        };
        SetupMockRepo(new List<TblCoupon> { coupon });

        // Act
        var result = await _couponService.ValidateCouponAsync("VALID", 200000);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    private static Mock<Microsoft.EntityFrameworkCore.DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();
        var dbSetMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();

        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        
        dbSetMock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        return dbSetMock;
    }

    private class TestAsyncQueryProvider<TEntity> : Microsoft.EntityFrameworkCore.Query.IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object? Execute(System.Linq.Expressions.Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) => _inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
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
