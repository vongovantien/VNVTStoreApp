using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Domain.Entities;
using System;
using System.Collections.Generic;

namespace VNVTStore.Tests.Common;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        // Seed basic master data to satisfy foreign key constraints
        SeedMasterData(context);

        return context;
    }

    private static void SeedMasterData(ApplicationDbContext context)
    {
        // Roles
        if (!context.TblRoles.Any())
        {
            context.TblRoles.AddRange(
                new TblRole { Code = "ADMIN", Name = "Admin" },
                new TblRole { Code = "CUSTOMER", Name = "Customer" }
            );
        }

        // Categories
        if (!context.TblCategories.Any())
        {
            context.TblCategories.Add(new TblCategory { Code = "CAT01", Name = "Category 1" });
            context.TblCategories.Add(new TblCategory { Code = "C1", Name = "C1" });
        }

        // Suppliers
        if (!context.TblSuppliers.Any())
        {
            context.TblSuppliers.Add(new TblSupplier { Code = "SUP01", Name = "Supplier 1", IsActive = true });
            context.TblSuppliers.Add(new TblSupplier { Code = "S1", Name = "S1", IsActive = true });
        }

        // Brands
        if (!context.TblBrands.Any())
        {
            context.TblBrands.Add(new TblBrand { Code = "BRAND01", Name = "Brand 1", IsActive = true });
            context.TblBrands.Add(new TblBrand { Code = "B1", Name = "B1", IsActive = true });
        }

        context.SaveChanges();
    }

    public static void Destroy(ApplicationDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}
