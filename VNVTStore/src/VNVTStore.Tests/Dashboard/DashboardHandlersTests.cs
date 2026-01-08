using AutoMapper;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.Dashboard.Handlers;
using VNVTStore.Application.Dashboard.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Tests.Extensions;
using Xunit;

namespace VNVTStore.Tests.Dashboard;

public class DashboardHandlersTests
{
    private readonly Mock<IRepository<TblOrder>> _orderRepoMock;
    private readonly Mock<IRepository<TblProduct>> _productRepoMock;
    private readonly Mock<IRepository<TblUser>> _userRepoMock;
    private readonly DashboardHandlers _handler;

    public DashboardHandlersTests()
    {
        _orderRepoMock = new Mock<IRepository<TblOrder>>();
        _productRepoMock = new Mock<IRepository<TblProduct>>();
        _userRepoMock = new Mock<IRepository<TblUser>>();
        
        _handler = new DashboardHandlers(
            _orderRepoMock.Object,
            _productRepoMock.Object,
            _userRepoMock.Object
        );
    }

    [Fact]
    public async Task GetDashboardStats_ReturnsValidStats()
    {
        // Arrange - Mock CountAsync
        _orderRepoMock.Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TblOrder, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        
        // Mock AsQueryable for ToListAsync calls
        var orders = new List<TblOrder>
        {
            new TblOrder { OrderDate = DateTime.UtcNow, FinalAmount = 100 },
            new TblOrder { OrderDate = DateTime.UtcNow.AddMonths(-1), FinalAmount = 90 }
        };
        _orderRepoMock.Setup(r => r.AsQueryable()).Returns(orders.BuildMock());

        _productRepoMock.Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TblProduct, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(50);
        _userRepoMock.Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TblUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(200);
        
        // Mock AsQueryable for User repo (used in Customers logic)
        _userRepoMock.Setup(r => r.AsQueryable()).Returns(new List<TblUser>().BuildMock());

        // Act
        var result = await _handler.Handle(new GetDashboardStatsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(100, result.Value.TotalOrders);
        Assert.Equal(50, result.Value.TotalProducts);
        Assert.Equal(200, result.Value.TotalCustomers);
    }
}
