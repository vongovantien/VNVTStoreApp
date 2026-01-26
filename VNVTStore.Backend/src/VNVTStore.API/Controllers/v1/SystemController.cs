using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using MediatR;
using VNVTStore.Application.Dashboard.Queries;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/system")]
public class SystemController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;

    public SystemController(ApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    [HttpGet("dashboard-debug")]
    public async Task<IActionResult> GetDashboardDebug()
    {
        var result = await _mediator.Send(new GetDashboardStatsQuery());
        return Ok(result);
    }

    [HttpGet("counts")]
    public async Task<IActionResult> GetCounts()
    {
        var latestOrder = await _context.TblOrders.OrderByDescending(o => o.OrderDate).FirstOrDefaultAsync();
        
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
        
        var thisMonthOrders = await _context.TblOrders
            .Where(o => o.OrderDate >= thisMonthStart)
            .ToListAsync();

        var adminUser = await _context.TblUsers.FirstOrDefaultAsync(u => u.Username == "admin");
        var userRoles = await _context.TblUsers
            .Take(10)
            .Select(u => new { u.Username, u.Role })
            .ToListAsync();
            
        var counts = new
        {
            AdminUser = adminUser != null ? new { adminUser.Username, Role = adminUser.Role.ToString() } : null,
            UsersCount = await _context.TblUsers.CountAsync(),
            SampleUserRoles = userRoles,
            Products = await _context.TblProducts.CountAsync(),
            Categories = await _context.TblCategories.CountAsync(),
            Orders = await _context.TblOrders.CountAsync(),
            TotalRevenue = await _context.TblOrders.SumAsync(o => o.FinalAmount),
            LatestOrderDate = latestOrder?.OrderDate,
            ThisMonthOrdersCount = thisMonthOrders.Count,
            UtcNow = now,
            ThisMonthStart = thisMonthStart,
            Banners = await _context.TblBanners.CountAsync(),
            Suppliers = await _context.Set<TblSupplier>().CountAsync()
        };

        return Ok(counts);
    }
}
