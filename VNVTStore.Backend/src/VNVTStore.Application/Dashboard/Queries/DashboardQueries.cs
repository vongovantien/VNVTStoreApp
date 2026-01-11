using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Dashboard.Queries;

public record GetDashboardStatsQuery() : IRequest<Result<DashboardStatsDto>>;

public record GetRevenueChartQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<IEnumerable<RevenueChartItem>>>;

public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueChange { get; set; } // Percentage change from previous period
    public int TotalOrders { get; set; }
    public decimal OrdersChange { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public decimal CustomersChange { get; set; }
    public int PendingOrders { get; set; }
    public int PendingQuotes { get; set; }
}

public class RevenueChartItem
{
    public string Label { get; set; } = null!;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
