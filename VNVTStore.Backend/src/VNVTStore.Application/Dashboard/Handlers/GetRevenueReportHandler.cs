using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Common;
using VNVTStore.Application.Dashboard.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Dashboard.Handlers;

public class GetRevenueReportHandler : IRequestHandler<GetRevenueReportQuery, Result<RevenueReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetRevenueReportHandler> _logger;

    public GetRevenueReportHandler(IApplicationDbContext context, ILogger<GetRevenueReportHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<RevenueReportDto>> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            var endDate = request.EndDate.HasValue
                ? DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
                : now;
            var startDate = request.StartDate.HasValue
                ? DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc)
                : endDate.AddDays(-30);

            var duration = endDate - startDate;
            var prevStartDate = startDate - duration;
            var prevEndDate = startDate.AddTicks(-1);

            // Fetch orders in period with items and payment
            var orders = await _context.TblOrders
                .Include(o => o.TblPayment)
                .Include(o => o.TblOrderItems)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .ToListAsync(cancellationToken);

            // Fetch previous period orders for comparison
            var prevOrders = await _context.TblOrders
                .Where(o => o.CreatedAt >= prevStartDate && o.CreatedAt <= prevEndDate)
                .ToListAsync(cancellationToken);

            // Summary metrics
            var totalOrders = orders.Count;
            var completedOrders = orders.Count(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed);
            var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);
            var pendingOrders = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Processing);

            var totalRevenue = orders
                .Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed)
                .Sum(o => o.FinalAmount);

            var totalDiscount = orders.Sum(o => o.DiscountAmount ?? 0);
            var aov = completedOrders > 0 ? totalRevenue / completedOrders : 0;

            var prevRevenue = prevOrders
                .Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed)
                .Sum(o => o.FinalAmount);

            var revenueChange = CalculateChange(totalRevenue, prevRevenue);
            var ordersChange = CalculateChange(totalOrders, prevOrders.Count);

            var summary = new RevenueSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = aov,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                PendingOrders = pendingOrders,
                TotalDiscount = totalDiscount,
                RevenueChangeVsPreviousPeriod = revenueChange,
                OrdersChangeVsPreviousPeriod = ordersChange
            };

            // Timeline breakdown
            var timeline = new List<RevenueTimelineDto>();
            var isMonthly = (request.GroupBy?.ToLower() == "month") || (endDate - startDate).TotalDays > 45;

            if (isMonthly)
            {
                var cur = new DateTime(startDate.Year, startDate.Month, 1);
                var endMonth = new DateTime(endDate.Year, endDate.Month, 1);
                while (cur <= endMonth)
                {
                    var nextMonth = cur.AddMonths(1);
                    var monthOrders = orders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value >= cur && o.CreatedAt.Value < nextMonth).ToList();
                    var monthRevenue = monthOrders.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed).Sum(o => o.FinalAmount);

                    timeline.Add(new RevenueTimelineDto
                    {
                        Date = cur.ToString("yyyy-MM"),
                        Label = cur.ToString("MM/yyyy"),
                        Revenue = monthRevenue,
                        TotalOrders = monthOrders.Count,
                        CompletedOrders = monthOrders.Count(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed),
                        CancelledOrders = monthOrders.Count(o => o.Status == OrderStatus.Cancelled)
                    });
                    cur = nextMonth;
                }
            }
            else
            {
                var totalDays = (int)(endDate.Date - startDate.Date).TotalDays + 1;
                for (int i = 0; i < totalDays; i++)
                {
                    var day = startDate.Date.AddDays(i);
                    var dayOrders = orders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == day).ToList();
                    var dayRevenue = dayOrders.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed).Sum(o => o.FinalAmount);

                    timeline.Add(new RevenueTimelineDto
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        Label = day.ToString("dd/MM"),
                        Revenue = dayRevenue,
                        TotalOrders = dayOrders.Count,
                        CompletedOrders = dayOrders.Count(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed),
                        CancelledOrders = dayOrders.Count(o => o.Status == OrderStatus.Cancelled)
                    });
                }
            }

            // Top Products
            var topProducts = orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SelectMany(o => o.TblOrderItems)
                .GroupBy(oi => oi.ProductName)
                .Select(g => new TopProductDto
                {
                    Name = g.Key ?? "Unknown",
                    Sales = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.PriceAtOrder * oi.Quantity)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(10)
                .ToList();

            // Payment Methods breakdown
            var paymentGroups = orders
                .GroupBy(o => o.TblPayment != null ? o.TblPayment.Method.ToString() : "COD")
                .Select(g => new PaymentMethodStatDto
                {
                    Method = g.Key,
                    Revenue = g.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed).Sum(o => o.FinalAmount),
                    OrderCount = g.Count(),
                    Percentage = totalOrders > 0 ? Math.Round((double)g.Count() / totalOrders * 100, 1) : 0
                })
                .OrderByDescending(p => p.OrderCount)
                .ToList();

            // Order Statuses breakdown
            var statusGroups = orders
                .GroupBy(o => o.Status.ToString())
                .Select(g => new OrderStatusStatDto
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(o => o.FinalAmount),
                    Percentage = totalOrders > 0 ? Math.Round((double)g.Count() / totalOrders * 100, 1) : 0
                })
                .OrderByDescending(s => s.Count)
                .ToList();

            return Result.Success(new RevenueReportDto
            {
                Summary = summary,
                Timeline = timeline,
                TopProducts = topProducts,
                PaymentMethods = paymentGroups,
                OrderStatuses = statusGroups
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching revenue report");
            return Result.Failure<RevenueReportDto>(Error.Validation("Failed to fetch revenue report"));
        }
    }

    private static double CalculateChange(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return (double)Math.Round((current - previous) / previous * 100, 1);
    }

    private static double CalculateChange(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return (double)Math.Round((float)(current - previous) / previous * 100, 1);
    }
}
