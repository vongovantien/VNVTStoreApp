using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Dashboard.Queries;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Application.Dashboard.Handlers;

public class GetSystemCountsHandler : IRequestHandler<GetSystemCountsQuery, GetSystemCountsResult>
{
    private readonly IApplicationDbContext _context;

    public GetSystemCountsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetSystemCountsResult> Handle(GetSystemCountsQuery request, CancellationToken cancellationToken)
    {
        var latestOrder = await _context.TblOrders
            .OrderByDescending(o => o.OrderDate)
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);

        var thisMonthOrders = await _context.TblOrders
            .Where(o => o.OrderDate >= thisMonthStart)
            .ToListAsync(cancellationToken);

        var adminUser = await _context.TblUsers
            .FirstOrDefaultAsync(u => u.Username == "admin", cancellationToken);

        var userRoles = await _context.TblUsers
            .Take(10)
            .Select(u => new { u.Username, u.Role })
            .ToListAsync(cancellationToken);

        return new GetSystemCountsResult
        {
            AdminUser = adminUser != null
                ? new { adminUser.Username, Role = adminUser.Role.ToString() }
                : null,
            UsersCount = await _context.TblUsers.CountAsync(cancellationToken),
            SampleUserRoles = userRoles.Cast<object>(),
            Products = await _context.TblProducts.CountAsync(cancellationToken),
            Categories = await _context.TblCategories.CountAsync(cancellationToken),
            Orders = await _context.TblOrders.CountAsync(cancellationToken),
            TotalRevenue = await _context.TblOrders.SumAsync(o => o.FinalAmount, cancellationToken),
            LatestOrderDate = latestOrder?.OrderDate,
            ThisMonthOrdersCount = thisMonthOrders.Count,
            UtcNow = now,
            ThisMonthStart = thisMonthStart,
            Banners = await _context.TblBanners.CountAsync(cancellationToken),
            Suppliers = await _context.TblSuppliers.CountAsync(cancellationToken)
        };
    }
}
