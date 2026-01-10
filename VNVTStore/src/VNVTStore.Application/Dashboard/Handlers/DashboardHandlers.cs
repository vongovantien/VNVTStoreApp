using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Dashboard.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Dashboard.Handlers;

public class DashboardHandlers :
    IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IRepository<TblOrder> _orderRepository;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IRepository<TblUser> _userRepository;

    public DashboardHandlers(
        IRepository<TblOrder> orderRepository,
        IRepository<TblProduct> productRepository,
        IRepository<TblUser> userRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        // This month orders
        var thisMonthOrders = await _orderRepository.AsQueryable()
            .Where(o => o.OrderDate >= thisMonthStart)
            .ToListAsync(cancellationToken);

        // Last month orders
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

        var totalCustomers = await _userRepository.CountAsync(u => u.Role == "customer", cancellationToken);
        
        // Customers this month
        var thisMonthCustomers = await _userRepository.CountAsync(
            u => u.Role == "customer" && u.CreatedAt >= thisMonthStart, cancellationToken);
        var lastMonthCustomers = await _userRepository.CountAsync(
            u => u.Role == "customer" && u.CreatedAt >= lastMonthStart && u.CreatedAt < thisMonthStart, cancellationToken);
        var customersChange = lastMonthCustomers > 0 
            ? ((decimal)(thisMonthCustomers - lastMonthCustomers) / lastMonthCustomers * 100) 
            : 0;

        var pendingOrders = await _orderRepository.CountAsync(o => o.Status == "Pending", cancellationToken);

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
            PendingQuotes = 0 // Placeholder for quote feature
        });
    }
}
