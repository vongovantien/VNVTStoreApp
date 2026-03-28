using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using Serilog;

namespace VNVTStore.Application.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        // 1. Seed Admin User
        var adminExists = await context.TblUsers.AnyAsync(u => u.Username == "admin");
        if (!adminExists)
        {
            var admin = TblUser.Create(
                "admin",
                "admin@vnvtstore.com",
                passwordHasher.Hash("Admin@123"),
                "System Administrator",
                UserRole.Admin
            );
            context.TblUsers.Add(admin);
            Log.Information("[SeedAsync] Seeded Admin User: admin / Admin@123");
        }

        // 2. Seed Base Data (Category, Brand, Unit)
        var category = await context.TblCategories.FirstOrDefaultAsync();
        if (category == null)
        {
            category = new TblCategory { Code = "CAT001", Name = "General", IsActive = true };
            context.TblCategories.Add(category);
        }

        var brand = await context.TblBrands.FirstOrDefaultAsync();
        if (brand == null)
        {
            brand = new TblBrand { Code = "BRAND001", Name = "Generic Brand", IsActive = true };
            context.TblBrands.Add(brand);
        }

        var unit = await context.TblUnits.FirstOrDefaultAsync();
        if (unit == null)
        {
            unit = new TblUnit { Code = "PCS", Name = "Cái" };
            context.TblUnits.Add(unit);
        }

        await context.SaveChangesAsync(default);

        // 3. Seed Staff Roles and Users for AccessChannel Testing
        var webStaffRole = await context.TblRoles.FirstOrDefaultAsync(r => r.Code == "WEB_STAFF");
        if (webStaffRole == null)
        {
            webStaffRole = new TblRole
            {
                Code = "WEB_STAFF",
                Name = "Web Staff",
                Description = "Staff with Web Access Only",
                IsActive = true
            };
            context.TblRoles.Add(webStaffRole);
        }

        var posStaffRole = await context.TblRoles.FirstOrDefaultAsync(r => r.Code == "POS_STAFF");
        if (posStaffRole == null)
        {
            posStaffRole = new TblRole
            {
                Code = "POS_STAFF",
                Name = "POS Staff",
                Description = "Staff with POS Access Only",
                IsActive = true
            };
            context.TblRoles.Add(posStaffRole);
        }
        
        // Seed Users for these roles
        // Note: Using TblUser.Create and then updating RoleCode manually because Factory uses Enum
        var webUser = await context.TblUsers.FirstOrDefaultAsync(u => u.Username == "webuser");
        if (webUser == null)
        {
            var user = TblUser.Create(
                "webuser",
                "webuser@vnvtstore.com",
                passwordHasher.Hash("Staff@123"),
                "Web Staff User",
                UserRole.Staff
            );
            user.UpdateRole("WEB_STAFF");
            context.TblUsers.Add(user);
            Log.Information("[SeedAsync] Seeded Web Staff: webuser / Staff@123");
        }

        var posUser = await context.TblUsers.FirstOrDefaultAsync(u => u.Username == "posuser");
        if (posUser == null)
        {
            var user = TblUser.Create(
                "posuser",
                "posuser@vnvtstore.com",
                passwordHasher.Hash("Staff@123"),
                "POS Staff User",
                UserRole.Staff
            );
            user.UpdateRole("POS_STAFF");
            context.TblUsers.Add(user);
            Log.Information("[SeedAsync] Seeded POS Staff: posuser / Staff@123");
        }
        
        // 3.5. Seed Default System Secrets
        var defaultSecrets = new[]
        {
            new TblSystemSecret { Code = "EMAIL_HOST", SecretValue = "smtp.gmail.com", Description = "SMTP Server Host", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "EMAIL_PORT", SecretValue = "587", Description = "SMTP Server Port", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "EMAIL_FROM", SecretValue = "your-email@gmail.com", Description = "Sender Email Address", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "EMAIL_PASSWORD", SecretValue = "your-app-password", Description = "Sender Email Password (App Password)", IsActive = true, IsEncrypted = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "EMAIL_SSL", SecretValue = "true", Description = "Enable SSL for SMTP", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "FRONTEND_URL", SecretValue = "http://localhost:5173", Description = "Frontend Application URL", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "ADMIN_EMAIL", SecretValue = "admin@vnvtstore.com", Description = "System Administrator Email", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "LOYALTY_POINTS_PER_CURRENCY", SecretValue = "10000", Description = "Currency amount required to earn 1 point", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "LOYALTY_TIER_LOYAL_THRESHOLD", SecretValue = "1000", Description = "Points required for LOYAL tier", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "LOYALTY_TIER_VIP_THRESHOLD", SecretValue = "5000", Description = "Points required for VIP tier", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "CLOUDINARY_CLOUD_NAME", SecretValue = "dgct8zpvp", Description = "Cloudinary Cloud Name", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "CLOUDINARY_API_KEY", SecretValue = "631943736442228", Description = "Cloudinary API Key", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "CLOUDINARY_API_SECRET", SecretValue = "4xKADk_UWqAKS7vlOl8qst_LUjw", Description = "Cloudinary API Secret", IsActive = true, IsEncrypted = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TblSystemSecret { Code = "FIREBASE_KEY", SecretValue = "", Description = "Firebase Service Account JSON", IsActive = true, IsEncrypted = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        foreach (var secret in defaultSecrets)
        {
            if (!await context.TblSystemSecrets.AnyAsync(s => s.Code == secret.Code))
            {
                context.TblSystemSecrets.Add(secret);
            }
        }

        await context.SaveChangesAsync(default);

        // 4. Seed 100 Products
        var productCount = await context.TblProducts.CountAsync();
        if (productCount < 100)
        {
            int productsToCreate = 100 - productCount;
            var random = new Random();

            for (int i = 1; i <= productsToCreate; i++)
            {
                var productCode = $"PRD{productCount + i:D3}";
                var product = TblProduct.Create(
                    name: $"Sản phẩm mẫu {productCount + i}",
                    price: random.Next(100000, 5000000),
                    wholesalePrice: random.Next(80000, 4000000),
                    stock: random.Next(10, 500),
                    categoryCode: category.Code,
                    costPrice: random.Next(50000, 3000000),
                    supplierCode: null,
                    brandCode: brand.Code,
                    baseUnit: unit.Code
                );

                // Use the code generated by factory or set it if needed
                // factory method sets Guid, but if we want sequential:
                // product.Code = productCode; // Set sequential code for easier tracking

                context.TblProducts.Add(product);

                // Add Unit
                context.TblProductUnits.Add(new TblProductUnit
                {
                    Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                    ProductCode = product.Code,
                    UnitCode = unit.Code,
                    ConversionRate = 1,
                    Price = product.Price,
                    IsBaseUnit = true
                });

                // Add Details
                context.TblProductDetails.Add(new TblProductDetail
                {
                    Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                    ProductCode = product.Code,
                    DetailType = ProductDetailType.SPEC,
                    SpecName = "Chất liệu",
                    SpecValue = "Cao cấp"
                });

                context.TblProductDetails.Add(new TblProductDetail
                {
                    Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                    ProductCode = product.Code,
                    DetailType = ProductDetailType.SPEC,
                    SpecName = "Xuất xứ",
                    SpecValue = "Việt Nam"
                });

                // Add Image (File record)
                var imageFile = TblFile.Create(
                    fileName: $"{product.Code}_main.jpg",
                    originalName: "product.jpg",
                    extension: ".jpg",
                    mimeType: "image/jpeg",
                    size: 50000,
                    path: $"https://picsum.photos/seed/{product.Code}/600/600",
                    masterCode: product.Code,
                    masterType: "TblProduct"
                );
                context.TblFiles.Add(imageFile);
            }

            await context.SaveChangesAsync(default);
            Log.Information("[SeedAsync] Successfully seeded {Count} products.", productsToCreate);
        }
    }
}
