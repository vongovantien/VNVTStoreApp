using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VNVTStore.Infrastructure.Persistence;

namespace VNVTStore.IntegrationTests;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=VNVTStore_Test;Username=postgres;Password=postgres"
            });
            config.AddJsonFile("appsettings.json")
                  .AddEnvironmentVariables();
        });

        builder.ConfigureServices((context, services) =>
        {
            // The DB context is already added in Infrastructure, 
            // but we can override it here if we want to use a different provider or connection string.
            // However, by adding a custom appsettings.json, Infrastructure will already use the test DB.
            
            // Ensure schema is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var connectionString = db.Database.GetDbConnection().ConnectionString;
            Console.WriteLine($"[DEBUG] Test Connection String: {connectionString}");
            
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Seed Permissions & Roles
            VNVTStore.Application.Seeding.PermissionSeeder.SeedAsync(db).Wait();

            // Seed Admin User
            if (!db.TblUsers.Any(u => u.Username == "admin"))
            {
                var hasher = scope.ServiceProvider.GetRequiredService<VNVTStore.Application.Interfaces.IPasswordHasher>();
                var adminUser = VNVTStore.Domain.Entities.TblUser.Create(
                    "admin",
                    "admin@example.com",
                    hasher.Hash("password"),
                    "System Administrator",
                    VNVTStore.Domain.Enums.UserRole.Admin
                );
                
                db.TblUsers.Add(adminUser);
                db.SaveChanges();
            }
        });

        builder.UseEnvironment("Development");
    }
}
