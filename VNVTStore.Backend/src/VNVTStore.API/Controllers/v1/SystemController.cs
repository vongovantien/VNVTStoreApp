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

    /// <summary>
    /// Multi-entity global search across products, orders, and customers
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> GlobalSearch([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new
            {
                success = true,
                data = new
                {
                    products = Array.Empty<object>(),
                    orders = Array.Empty<object>(),
                    customers = Array.Empty<object>()
                }
            });
        }

        var keyword = q.Trim().ToLower();

        // 1. Products
        var products = await _context.TblProducts
            .AsNoTracking()
            .Where(p => p.IsActive && (
                p.Name.ToLower().Contains(keyword) || 
                p.Code.ToLower().Contains(keyword)))
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new
            {
                code = p.Code,
                name = p.Name,
                price = p.Price,
                imageUrl = p.TblProductDetails.Where(d => d.SpecName == "image").Select(d => d.SpecValue).FirstOrDefault() ?? "",
                stock = p.StockQuantity ?? 0
            })
            .ToListAsync();

        // 2. Orders
        var orders = await _context.TblOrders
            .AsNoTracking()
            .Include(o => o.AddressCodeNavigation)
            .Include(o => o.UserCodeNavigation)
            .Where(o => o.IsActive && (
                o.Code.ToLower().Contains(keyword) || 
                (o.AddressCodeNavigation != null && o.AddressCodeNavigation.FullName != null && o.AddressCodeNavigation.FullName.ToLower().Contains(keyword)) ||
                (o.AddressCodeNavigation != null && o.AddressCodeNavigation.Phone != null && o.AddressCodeNavigation.Phone.Contains(keyword)) ||
                (o.UserCodeNavigation != null && o.UserCodeNavigation.FullName != null && o.UserCodeNavigation.FullName.ToLower().Contains(keyword))))
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new
            {
                code = o.Code,
                orderNumber = "#" + o.Code,
                customerName = o.AddressCodeNavigation != null ? o.AddressCodeNavigation.FullName : (o.UserCodeNavigation != null ? o.UserCodeNavigation.FullName : "Khách hàng"),
                totalAmount = o.FinalAmount,
                status = o.Status.ToString(),
                createdAt = o.CreatedAt.HasValue ? o.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm") : ""
            })
            .ToListAsync();

        // 3. Customers / Users
        var customers = await _context.TblUsers
            .AsNoTracking()
            .Where(u => u.IsActive && (
                (u.FullName != null && u.FullName.ToLower().Contains(keyword)) || 
                u.Username.ToLower().Contains(keyword) || 
                u.Email.ToLower().Contains(keyword) ||
                (u.Phone != null && u.Phone.Contains(keyword))))
            .Take(5)
            .Select(u => new
            {
                code = u.Code,
                fullName = u.FullName ?? u.Username,
                email = u.Email,
                phone = u.Phone,
                totalSpent = (decimal?)0
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                products,
                orders,
                customers
            }
        });
    }
}
