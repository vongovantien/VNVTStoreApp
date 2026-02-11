using Xunit;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;

namespace VNVTStore.Tests.Integration;

public class DbDiagnostics
{
    [Fact(Skip = "Remote DB not available")]
    public async Task CheckDatabaseCounts()
    {
        var connectionString = "Host=ep-shiny-hill-a1z2i02j.ap-southeast-1.aws.neon.tech;Database=saleapp;Username=saleapp_owner;Password=yp8tDSnX6vjF;SSL Mode=Require";
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        using var context = new ApplicationDbContext(optionsBuilder.Options);

        try 
        {
            await context.Database.CanConnectAsync();
            Console.WriteLine("--- DB COUNTS ---");
            var users = await context.TblUsers.CountAsync();
            var products = await context.TblProducts.CountAsync();
            var orders = await context.TblOrders.CountAsync();
            var categories = await context.TblCategories.CountAsync();
            var banners = await context.TblBanners.CountAsync();

            Console.WriteLine($"Users: {users}");
            Console.WriteLine($"Products: {products}");
            Console.WriteLine($"Orders: {orders}");
            Console.WriteLine($"Categories: {categories}");
            Console.WriteLine($"Banners: {banners}");
        }
        catch (Exception ex)
        {
             Console.WriteLine($"ERROR CONNECTING TO DB: {ex.Message}");
             Console.WriteLine(ex.StackTrace);
             throw;
        }

        Assert.True(true);
    }
}
