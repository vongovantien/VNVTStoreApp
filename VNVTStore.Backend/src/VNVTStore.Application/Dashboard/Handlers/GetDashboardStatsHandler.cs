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
            DateTime startDate;
            DateTime endDate;
            DateTime prevStartDate;
            DateTime prevEndDate;
            bool isCustomRange = request.StartDate.HasValue && request.EndDate.HasValue;

            if (isCustomRange)
            {
                startDate = DateTime.SpecifyKind(request.StartDate!.Value.Date, DateTimeKind.Utc);
                endDate = DateTime.SpecifyKind(request.EndDate!.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                var duration = endDate - startDate;
                prevStartDate = startDate - duration;
                prevEndDate = startDate.AddTicks(-1);
            }
            else
            {
                startDate = new DateTime(now.Year, now.Month, 1);
                endDate = now;
                prevStartDate = startDate.AddMonths(-1);
                prevEndDate = startDate.AddTicks(-1);
            }

            // 1. Basic Counts
            var totalOrders = isCustomRange
                ? await _context.TblOrders.CountAsync(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate, cancellationToken)
                : await _context.TblOrders.CountAsync(cancellationToken);

            var totalProducts = await _context.TblProducts.CountAsync(cancellationToken);
            var totalCustomers = await _context.TblUsers.CountAsync(u => u.RoleCode == "CUSTOMER" || string.IsNullOrEmpty(u.RoleCode), cancellationToken);

            var totalRevenue = isCustomRange
                ? await _context.TblOrders
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed))
                    .SumAsync(o => o.FinalAmount, cancellationToken)
                : await _context.TblOrders
                    .Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed)
                    .SumAsync(o => o.FinalAmount, cancellationToken);

            var pendingQuotes = await _context.TblQuotes.CountAsync(q => q.Status == "Pending", cancellationToken);

            // 2. Change Metrics (Current Period vs Previous Period)
            var revenueCurrentPeriod = await _context.TblOrders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed))
                .SumAsync(o => o.FinalAmount, cancellationToken);

            var revenuePrevPeriod = await _context.TblOrders
                .Where(o => o.CreatedAt >= prevStartDate && o.CreatedAt <= prevEndDate && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed))
                .SumAsync(o => o.FinalAmount, cancellationToken);
            
            var revenueChange = CalculateChange(revenueCurrentPeriod, revenuePrevPeriod);

            var ordersCurrentPeriod = await _context.TblOrders.CountAsync(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate, cancellationToken);
            var ordersPrevPeriod = await _context.TblOrders.CountAsync(o => o.CreatedAt >= prevStartDate && o.CreatedAt <= prevEndDate, cancellationToken);
            var ordersChange = CalculateChange(ordersCurrentPeriod, ordersPrevPeriod);

            var customersCurrentPeriod = await _context.TblUsers.CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate && (u.RoleCode == "CUSTOMER" || string.IsNullOrEmpty(u.RoleCode)), cancellationToken);
            var customersPrevPeriod = await _context.TblUsers.CountAsync(u => u.CreatedAt >= prevStartDate && u.CreatedAt <= prevEndDate && (u.RoleCode == "CUSTOMER" || string.IsNullOrEmpty(u.RoleCode)), cancellationToken);
            var customersChange = CalculateChange(customersCurrentPeriod, customersPrevPeriod);

            // 3. Top Products (by Quantity Sold)
            var topProductsQuery = _context.TblOrderItems
                .Include(oi => oi.OrderCodeNavigation)
                .Where(oi => oi.OrderCodeNavigation.Status != OrderStatus.Cancelled);

            if (isCustomRange)
            {
                topProductsQuery = topProductsQuery.Where(oi => oi.OrderCodeNavigation.CreatedAt >= startDate && oi.OrderCodeNavigation.CreatedAt <= endDate);
            }

            var topProducts = await topProductsQuery
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

            // 4. Revenue Chart
            var revenueChart = new List<RevenueChartDto>();
            if (isCustomRange)
            {
                var daysDiff = (int)(endDate.Date - startDate.Date).TotalDays;
                if (daysDiff <= 45)
                {
                    var dateList = Enumerable.Range(0, daysDiff + 1).Select(i => startDate.Date.AddDays(i)).ToList();
                    var periodOrders = await _context.TblOrders
                        .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.Status != OrderStatus.Cancelled)
                        .Select(o => new { CreatedAt = o.CreatedAt, FinalAmount = o.FinalAmount })
                        .ToListAsync(cancellationToken);

                    foreach (var date in dateList)
                    {
                        var dailyOrders = periodOrders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == date).ToList();
                        revenueChart.Add(new RevenueChartDto
                        {
                            Label = date.ToString("dd/MM"),
                            Revenue = dailyOrders.Sum(o => o.FinalAmount),
                            OrderCount = dailyOrders.Count
                        });
                    }
                }
                else
                {
                    // Monthly points
                    var cur = new DateTime(startDate.Year, startDate.Month, 1);
                    var endMonth = new DateTime(endDate.Year, endDate.Month, 1);
                    var periodOrders = await _context.TblOrders
                        .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.Status != OrderStatus.Cancelled)
                        .Select(o => new { CreatedAt = o.CreatedAt, FinalAmount = o.FinalAmount })
                        .ToListAsync(cancellationToken);

                    while (cur <= endMonth)
                    {
                        var next = cur.AddMonths(1);
                        var mOrders = periodOrders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value >= cur && o.CreatedAt.Value < next).ToList();
                        revenueChart.Add(new RevenueChartDto
                        {
                            Label = cur.ToString("MM/yyyy"),
                            Revenue = mOrders.Sum(o => o.FinalAmount),
                            OrderCount = mOrders.Count
                        });
                        cur = next;
                    }
                }
            }
            else
            {
                var last7Days = Enumerable.Range(0, 7).Select(i => now.Date.AddDays(-i)).Reverse().ToList();
                var sevenDaysAgo = now.Date.AddDays(-6);
                var recentOrders = await _context.TblOrders
                    .Where(o => o.CreatedAt >= sevenDaysAgo && o.Status != OrderStatus.Cancelled)
                    .Select(o => new { CreatedAt = o.CreatedAt, FinalAmount = o.FinalAmount })
                    .ToListAsync(cancellationToken);

                foreach (var date in last7Days)
                {
                    var dailyOrders = recentOrders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == date).ToList();
                    revenueChart.Add(new RevenueChartDto
                    {
                        Label = date.ToString("dd/MM"),
                        Revenue = dailyOrders.Sum(o => o.FinalAmount),
                        OrderCount = dailyOrders.Count
                    });
                }
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
