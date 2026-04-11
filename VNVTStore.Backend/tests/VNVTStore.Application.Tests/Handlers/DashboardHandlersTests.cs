using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Dashboard.Handlers;
using VNVTStore.Application.Dashboard.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Tests.Helpers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Enums;
using Xunit;

namespace VNVTStore.Application.Tests.Handlers;

public class DashboardHandlersTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<GetDashboardStatsHandler>> _loggerStatsMock;

    public DashboardHandlersTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerStatsMock = new Mock<ILogger<GetDashboardStatsHandler>>();
        
        // Initialize all tables to avoid NullReferenceException in Handler
        _contextMock.Setup(x => x.TblProducts).Returns(TestingUtils.CreateMockDbSet(new List<TblProduct>()).Object);
        _contextMock.Setup(x => x.TblUsers).Returns(TestingUtils.CreateMockDbSet(new List<TblUser>()).Object);
        _contextMock.Setup(x => x.TblQuotes).Returns(TestingUtils.CreateMockDbSet(new List<TblQuote>()).Object);
        _contextMock.Setup(x => x.TblOrderItems).Returns(TestingUtils.CreateMockDbSet(new List<TblOrderItem>()).Object);
        _contextMock.Setup(x => x.TblCategories).Returns(TestingUtils.CreateMockDbSet(new List<TblCategory>()).Object);
        _contextMock.Setup(x => x.TblBanners).Returns(TestingUtils.CreateMockDbSet(new List<TblBanner>()).Object);
        _contextMock.Setup(x => x.TblSuppliers).Returns(TestingUtils.CreateMockDbSet(new List<TblSupplier>()).Object);
    }

    [Fact]
    public async Task Handle_GetSystemCounts_ShouldReturnCorrectCounts()
    {
        // Arrange
        var orders = new List<TblOrder> { 
            TblOrder.Create("U1", "A1", 100, 0, 0, null),
            TblOrder.Create("U2", "A2", 200, 0, 0, null)
        };
        _contextMock.Setup(x => x.TblOrders).Returns(TestingUtils.CreateMockDbSet(orders).Object);

        var handler = new GetSystemCountsHandler(_contextMock.Object);

        // Act
        var result = await handler.Handle(new GetSystemCountsQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Orders);
    }

    [Fact]
    public async Task Handle_GetDashboardStats_ShouldReturnAggregatedRevenue()
    {
        // Arrange
        var today = DateTime.UtcNow;
        var orders = new List<TblOrder> { 
            TblOrder.Create("U1", "A1", 1000000, 0, 0, null),
            TblOrder.Create("U2", "A2", 500000, 0, 0, null)
        };
        
        foreach(var o in orders) {
            o.GetType().GetProperty("OrderDate")?.SetValue(o, today);
            o.GetType().GetProperty("CreatedAt")?.SetValue(o, today);
            o.UpdateStatus(OrderStatus.Completed);
        }

        _contextMock.Setup(x => x.TblOrders).Returns(TestingUtils.CreateMockDbSet(orders).Object);

        var handler = new GetDashboardStatsHandler(_contextMock.Object, _loggerStatsMock.Object);

        // Act
        var result = await handler.Handle(new GetDashboardStatsQuery(), CancellationToken.None);

        // Assert
        if (!result.IsSuccess)
            throw new Exception($"Handler Failed: {result.Error?.Message}");

        Assert.True(result.IsSuccess);
        Assert.Equal(1500000, result.Value!.TotalRevenue);
    }
}
