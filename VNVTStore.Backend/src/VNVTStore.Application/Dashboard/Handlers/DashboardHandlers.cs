using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Dashboard.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Dashboard.Handlers;

public class DashboardHandlers :
    IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IRepository<TblOrder> _orderRepository;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IRepository<TblUser> _userRepository;
    private readonly IRepository<TblQuote> _quoteRepository;

    public DashboardHandlers(
        IRepository<TblOrder> orderRepository,
        IRepository<TblProduct> productRepository,
        IRepository<TblUser> userRepository,
        IRepository<TblQuote> quoteRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _quoteRepository = quoteRepository;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        // Fetch orders for stats
        // To optimize, we could use separate count queries vs fetching all data. 
        // For now, let's keep it simple as implemented before but robust.
        
        var thisMonthOrders = await _orderRepository.AsQueryable()
            .Where(o => o.OrderDate >= thisMonthStart)
            .ToListAsync(cancellationToken);

        var lastMonthOrders = await _orderRepository.AsQueryable()
            .Where(o => o.OrderDate >= lastMonthStart && o.OrderDate < thisMonthStart)
            .ToListAsync(cancellationToken);

        var totalRevenue = thisMonthOrders.Sum(o => o.FinalAmount);
        var lastMonthRevenue = lastMonthOrders.Sum(o => o.FinalAmount);
        var revenueChange = lastMonthRevenue > 0 
            ? ((totalRevenue - lastMonthRevenue) / lastMonthRevenue * 100) 
            : 0;

        var totalOrders = await _orderRepository.CountAsync(null, cancellationToken);
        var thisMonthOrderCount = thisMonthOrders.Count;
        var lastMonthOrderCount = lastMonthOrders.Count;
        var ordersChange = lastMonthOrderCount > 0 
            ? ((decimal)(thisMonthOrderCount - lastMonthOrderCount) / lastMonthOrderCount * 100) 
            : 0;

        var totalProducts = await _productRepository.CountAsync(p => p.IsActive == true, cancellationToken);
        var totalCustomers = await _userRepository.CountAsync(u => u.Role == UserRole.Customer, cancellationToken);
        
        // Customers this month
        var thisMonthCustomers = await _userRepository.CountAsync(
            u => u.Role == UserRole.Customer && u.CreatedAt >= thisMonthStart, cancellationToken);
        var lastMonthCustomers = await _userRepository.CountAsync(
            u => u.Role == UserRole.Customer && u.CreatedAt >= lastMonthStart && u.CreatedAt < thisMonthStart, cancellationToken);
        var customersChange = lastMonthCustomers > 0 
            ? ((decimal)(thisMonthCustomers - lastMonthCustomers) / lastMonthCustomers * 100) 
            : 0;

        var pendingOrders = await _orderRepository.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var pendingQuotes = await _quoteRepository.CountAsync(q => q.Status == "Pending", cancellationToken); // Assuming status is string "Pending"

        return Result.Success(new DashboardStatsDto
        {
            TotalRevenue = totalRevenue,
            RevenueChange = revenueChange,
            TotalOrders = totalOrders,
            OrdersChange = ordersChange,
            TotalProducts = totalProducts,
            TotalCustomers = totalCustomers,
            CustomersChange = customersChange,
            PendingOrders = pendingOrders,
            PendingQuotes = pendingQuotes,
            TopProducts = await GetTopProducts(cancellationToken),
            RevenueChart = await GetRevenueChart(cancellationToken)
        });
    }

    private async Task<List<TopProductDto>> GetTopProducts(CancellationToken cancellationToken)
    {
        // Fetch recent completed orders with items
        var fromDate = DateTime.UtcNow.AddMonths(-3);
        
        // Note: Generic Repository might not support Include easily without specific methods.
        // Assuming AsQueryable allows Include if configured in DbContext, but TblOrder has virtual TblOrderItems.
        // We will fetch orders and then items if LazyLoading is on, or Eager Load.
        // Safer approach with generic repo: Get orders, then get items??? No, too many queries.
        // We will assume basic Include capability or fetch all items in timeframe (if possible).
        // Since we can't easily Include here without casting or extension methods which might not be available,
        // We will TRY to query assuming basic EF Core behavior on AsQueryable if lazy loading is enabled.
        
        // BETTER APPROACH: Query TblOrders, fetch IDs, then query items? No TblOrderItem repository available.
        // BACKTRACK: We need TblOrder to include TblOrderItems. 
        // If AsQueryable returns IQueryable<TblOrder>, we can use .Include() if we add "using Microsoft.EntityFrameworkCore;".
        
        var orders = await _orderRepository.AsQueryable()
            .Include(o => o.TblOrderItems)
            .Where(o => o.OrderDate >= fromDate && o.Status == OrderStatus.Completed) // Only count completed orders
            .ToListAsync(cancellationToken);

        var allItems = orders.SelectMany(o => o.TblOrderItems);

        var topProducts = allItems
            .GroupBy(x => x.ProductCode)
            .Select(g => new 
            {
                ProductCode = g.Key,
                Sales = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.PriceAtOrder)
            })
            .OrderByDescending(x => x.Sales)
            .Take(5)
            .ToList();

        // Fetch names
        if (!topProducts.Any()) return new List<TopProductDto>();

        var productCodes = topProducts.Select(x => x.ProductCode).ToList();
        // Handle potential null ProductCode if database allows, though entity says nullable.
        var safeCodes = productCodes.Where(c => c != null).Cast<string>().ToList();

        var products = await _productRepository.AsQueryable()
            .Where(p => safeCodes.Contains(p.Code))
            .Select(p => new { p.Code, p.Name })
            .ToListAsync(cancellationToken);

        return topProducts.Select(tp => new TopProductDto
        {
            Name = products.FirstOrDefault(p => p.Code == tp.ProductCode)?.Name ?? tp.ProductCode ?? "Unknown",
            Sales = tp.Sales,
            Revenue = tp.Revenue
        }).ToList();
    }

    private async Task<List<RevenueChartItem>> GetRevenueChart(CancellationToken cancellationToken)
    {
        var end = DateTime.UtcNow;
        var start = end.AddMonths(-11);
        
        // Simplification: Fetch all relevant orders and group in memory to avoid date grouping SQL issues across providers
        var orders = await _orderRepository.AsQueryable()
            .Where(o => o.OrderDate >= start && o.Status != OrderStatus.Cancelled)
            .Select(o => new { o.OrderDate, o.FinalAmount })
            .ToListAsync(cancellationToken);

        var grouped = orders
            .GroupBy(o => new { Month = o.OrderDate?.Month ?? 0, Year = o.OrderDate?.Year ?? 0 })
            .Where(g => g.Key.Month != 0)
            .Select(g => new RevenueChartItem
            {
                Label = $"{g.Key.Month}/{g.Key.Year}",
                Revenue = g.Sum(x => x.FinalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(x => DateTime.ParseExact(x.Label, "M/yyyy", System.Globalization.CultureInfo.InvariantCulture))
            .ToList();

        return grouped;
    }
}
