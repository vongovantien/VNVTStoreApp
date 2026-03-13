using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Common;
using VNVTStore.Application.Dashboard.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Dashboard.Handlers;

public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetDashboardStatsHandler> _logger;

    public GetDashboardStatsHandler(IApplicationDbContext context, ILogger<GetDashboardStatsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddTicks(-1);

            // 1. Basic Counts
            var totalOrders = await _context.TblOrders.CountAsync(cancellationToken);
            var totalProducts = await _context.TblProducts.CountAsync(cancellationToken);
            var totalCustomers = await _context.TblUsers.CountAsync(u => u.Role == UserRole.Customer, cancellationToken);
            var totalRevenue = await _context.TblOrders
                .Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed)
                .SumAsync(o => o.FinalAmount, cancellationToken);

            var pendingQuotes = await _context.TblQuotes.CountAsync(q => q.Status == "Pending", cancellationToken);

            // 2. Change Metrics (Current Month vs Last Month)
            // Revenue Change
            var revenueCurrentMonth = await _context.TblOrders
                .Where(o => o.CreatedAt >= startOfMonth && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed))
                .SumAsync(o => o.FinalAmount, cancellationToken);

            var revenueLastMonth = await _context.TblOrders
                .Where(o => o.CreatedAt >= startOfLastMonth && o.CreatedAt <= endOfLastMonth && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed))
                .SumAsync(o => o.FinalAmount, cancellationToken);
            
            var revenueChange = CalculateChange(revenueCurrentMonth, revenueLastMonth);

            // Orders Change
            var ordersCurrentMonth = await _context.TblOrders.CountAsync(o => o.CreatedAt >= startOfMonth, cancellationToken);
            var ordersLastMonth = await _context.TblOrders.CountAsync(o => o.CreatedAt >= startOfLastMonth && o.CreatedAt <= endOfLastMonth, cancellationToken);
            var ordersChange = CalculateChange(ordersCurrentMonth, ordersLastMonth);

            // Customers Change
            var customersCurrentMonth = await _context.TblUsers.CountAsync(u => u.CreatedAt >= startOfMonth && u.Role == UserRole.Customer, cancellationToken);
            var customersLastMonth = await _context.TblUsers.CountAsync(u => u.CreatedAt >= startOfLastMonth && u.CreatedAt <= endOfLastMonth && u.Role == UserRole.Customer, cancellationToken);
            var customersChange = CalculateChange(customersCurrentMonth, customersLastMonth);

            // 3. Top Products (by Quantity Sold)
            var topProducts = await _context.TblOrderItems
                .Include(oi => oi.OrderCodeNavigation)
                .Where(oi => oi.OrderCodeNavigation.Status != OrderStatus.Cancelled)
                .GroupBy(oi => oi.ProductName) 
                .Select(g => new TopProductDto
                {
                    Name = g.Key ?? "Unknown",
                    Sales = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.PriceAtOrder * oi.Quantity) 
                })
                .OrderByDescending(x => x.Sales)
                .Take(5)
                .ToListAsync(cancellationToken);

            // 4. Revenue Chart (Last 7 days)
            var last7Days = Enumerable.Range(0, 7).Select(i => now.Date.AddDays(-i)).Reverse().ToList();
            var revenueChart = new List<RevenueChartDto>();

            var sevenDaysAgo = now.Date.AddDays(-6);
             // Use anonymous type with nullable Date handling carefully
            var recentOrders = await _context.TblOrders
                .Where(o => o.CreatedAt >= sevenDaysAgo && o.Status != OrderStatus.Cancelled)
                .Select(o => new { CreatedAt = o.CreatedAt, FinalAmount = o.FinalAmount })
                .ToListAsync(cancellationToken);

            foreach (var date in last7Days)
            {
                // Fix: Handle nullable DateTime access in memory
                var dailyOrders = recentOrders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == date).ToList();
                
                revenueChart.Add(new RevenueChartDto
                {
                    Label = date.ToString("dd/MM"),
                    Revenue = dailyOrders.Sum(o => o.FinalAmount),
                    OrderCount = dailyOrders.Count
                });
            }

            return Result.Success(new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                RevenueChange = revenueChange,
                OrdersChange = ordersChange,
                CustomersChange = customersChange,
                PendingQuotes = pendingQuotes,
                TopProducts = topProducts,
                RevenueChart = revenueChart
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard stats");
            return Result.Failure<DashboardStatsDto>(Error.Validation("Failed to fetch dashboard stats"));
        }
    }

    private double CalculateChange(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return (double)((current - previous) / previous * 100);
    }
    
    private double CalculateChange(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return (double)((float)(current - previous) / previous * 100);
    }
}
