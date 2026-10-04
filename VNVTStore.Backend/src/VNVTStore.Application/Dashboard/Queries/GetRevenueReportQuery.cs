using System;
using System.Collections.Generic;
using MediatR;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Dashboard.Queries;

public record GetRevenueReportQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? GroupBy = "day" // "day" or "month"
) : IRequest<Result<RevenueReportDto>>;

public class RevenueReportDto
{
    public RevenueSummaryDto Summary { get; set; } = new();
    public List<RevenueTimelineDto> Timeline { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<PaymentMethodStatDto> PaymentMethods { get; set; } = new();
    public List<OrderStatusStatDto> OrderStatuses { get; set; } = new();
}

public class RevenueSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal TotalDiscount { get; set; }
    public double RevenueChangeVsPreviousPeriod { get; set; }
    public double OrdersChangeVsPreviousPeriod { get; set; }
}

public class RevenueTimelineDto
{
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD or YYYY-MM
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
}

public class PaymentMethodStatDto
{
    public string Method { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public double Percentage { get; set; }
}

public class OrderStatusStatDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public double Percentage { get; set; }
}
