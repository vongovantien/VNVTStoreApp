using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Dashboard.Queries;

public record GetDashboardStatsQuery() : IRequest<Result<DashboardStatsDto>>;

public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    
    // Changes vs previous period (e.g. last month)
    public double RevenueChange { get; set; }
    public double OrdersChange { get; set; }
    public double CustomersChange { get; set; }
    
    public int PendingQuotes { get; set; }

    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<RevenueChartDto> RevenueChart { get; set; } = new();
}

public class TopProductDto
{
    public string Name { get; set; } = string.Empty;
    public int Sales { get; set; }
    public decimal Revenue { get; set; }
}

public class RevenueChartDto
{
    public string Label { get; set; } = string.Empty; // Date or Month
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
