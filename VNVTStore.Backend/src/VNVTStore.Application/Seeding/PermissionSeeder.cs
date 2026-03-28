using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Seeding;

public static class PermissionSeeder
{
    public static async Task SeedAsync(IApplicationDbContext context)
    {
        // 1. Seed Permissions
        var permissionFields = typeof(Permissions).GetNestedTypes()
            .SelectMany(t => t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy))
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .ToList();

        var existingPermissions = await context.TblPermissions.ToDictionaryAsync(p => p.Name);

        foreach (var field in permissionFields)
        {
            var pName = field.GetValue(null)?.ToString();
            if (string.IsNullOrEmpty(pName)) continue;

            if (!existingPermissions.ContainsKey(pName))
            {
                var module = field.DeclaringType?.Name ?? "General";
                context.TblPermissions.Add(new TblPermission
                {
                    Code = pName.Replace("Permissions.", "").Replace(".", "_").ToUpper(),
                    Name = pName,
                    Module = module,
                    Description = $"Permission for {pName}"
                });
            }
        }

        await context.SaveChangesAsync(default);

        // 2. Seed Admin Role
        var adminRole = await context.TblRoles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
        if (adminRole == null)
        {
            adminRole = new TblRole
            {
                Code = "ADMIN",
                Name = "Administrator",
                Description = "System Administrator with full access",
                IsActive = true
            };
            context.TblRoles.Add(adminRole);
            await context.SaveChangesAsync(default);
        }

        // 2b. Seed Customer Role
        var customerRole = await context.TblRoles.FirstOrDefaultAsync(r => r.Code == "CUSTOMER");
        if (customerRole == null)
        {
            customerRole = new TblRole
            {
                Code = "CUSTOMER",
                Name = "Customer",
                Description = "Default customer role",
                IsActive = true
            };
            context.TblRoles.Add(customerRole);
            await context.SaveChangesAsync(default);
        }

        // 3. Assign all permissions to Admin Role
        var allPermissions = await context.TblPermissions.ToListAsync();
        var existingRolePermissions = await context.TblRolePermissions
            .Where(rp => rp.RoleCode == "ADMIN")
            .Select(rp => rp.PermissionCode)
            .ToListAsync();

        foreach (var p in allPermissions)
        {
            if (!existingRolePermissions.Contains(p.Code))
            {
                context.TblRolePermissions.Add(new TblRolePermission
                {
                    RoleCode = "ADMIN",
                    PermissionCode = p.Code
                });
            }
        }

        await context.SaveChangesAsync(default);
    }
}
