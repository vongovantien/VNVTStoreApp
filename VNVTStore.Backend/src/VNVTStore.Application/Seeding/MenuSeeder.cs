using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Seeding;

public static class MenuSeeder
{
    private static readonly List<(string Code, string Name, string Path, string GroupCode, string GroupName, string? Icon, int SortOrder)> Menus = new()
    {
        // Core Group
        ("DASHBOARD", "Dashboard", "/admin", "CORE", "Core", "LayoutDashboard", 1),
        ("ORDERS", "Orders", "/admin/orders", "CORE", "Core", "ShoppingCart", 2),
        ("CUSTOMERS", "Customers", "/admin/customers", "CORE", "Core", "Users", 3),
        
        // Inventory Group
        ("CATEGORIES", "Categories", "/admin/categories", "INVENTORY", "Inventory", "Folder", 10),
        ("PRODUCTS", "Products", "/admin/products", "INVENTORY", "Inventory", "Package", 11),
        ("SUPPLIERS", "Suppliers", "/admin/suppliers", "INVENTORY", "Inventory", "Building2", 12),
        ("BRANDS", "Brands", "/admin/brands", "INVENTORY", "Inventory", "Tag", 13),
        ("UNITS", "Units", "/admin/units", "INVENTORY", "Inventory", "Ruler", 14),
        
        // Marketing Group
        ("QUOTES", "Quotes", "/admin/quotes", "MARKETING", "Marketing", "FileText", 20),
        ("PROMOTIONS", "Promotions", "/admin/promotions", "MARKETING", "Marketing", "Package", 21),
        ("COUPONS", "Coupons", "/admin/coupons", "MARKETING", "Marketing", "Ticket", 22),
        ("BANNERS", "Banners", "/admin/banners", "MARKETING", "Marketing", "LayoutDashboard", 23),
        ("NEWS", "News", "/admin/news", "MARKETING", "Marketing", "FileText", 24),
        ("REVIEWS", "Reviews", "/admin/reviews", "MARKETING", "Marketing", "Star", 25),
        
        // System Group
        ("SETTINGS", "Settings", "/admin/settings", "SYSTEM", "System", "Settings", 30),
        ("ROLES", "Roles", "/admin/roles", "SYSTEM", "System", "Shield", 31),
    };

    public static async Task SeedAsync(IApplicationDbContext context)
    {
        // 1. Seed Menus
        var existingMenus = await context.TblMenus.ToDictionaryAsync(m => m.Code);

        foreach (var menu in Menus)
        {
            if (!existingMenus.ContainsKey(menu.Code))
            {
                context.TblMenus.Add(new TblMenu
                {
                    Code = menu.Code,
                    Name = menu.Name,
                    Path = menu.Path,
                    GroupCode = menu.GroupCode,
                    GroupName = menu.GroupName,
                    Icon = menu.Icon,
                    SortOrder = menu.SortOrder,
                    IsActive = true
                });
            }
        }

        await context.SaveChangesAsync(default);
        
        // 2. Seed ADMIN Role if not exists
        var adminRole = await context.TblRoles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
        if (adminRole == null)
        {
            adminRole = new TblRole
            {
                Code = "ADMIN",
                Name = "Administrator",
                Description = "System administrator with full access",
                IsActive = true
            };
            context.TblRoles.Add(adminRole);
            await context.SaveChangesAsync(default);
        }

        // 3. Assign all menus to ADMIN role
        if (adminRole != null)
        {
            var existingAdminMenus = await context.TblRoleMenus
                .Where(rm => rm.RoleCode == "ADMIN")
                .Select(rm => rm.MenuCode)
                .ToListAsync();

            var allMenuCodes = await context.TblMenus.Select(m => m.Code).ToListAsync();
            
            foreach (var menuCode in allMenuCodes)
            {
                if (!existingAdminMenus.Contains(menuCode))
                {
                    context.TblRoleMenus.Add(new TblRoleMenu
                    {
                        RoleCode = "ADMIN",
                        MenuCode = menuCode
                    });
                }
            }

            await context.SaveChangesAsync(default);
        }
    }
}
