using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Application.Interfaces;

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
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=VNVTStore_Test;Username=postgres;Password=password",
                ["JwtSettings:SecretKey"] = "SuperSecretKeyForIntegrationTestingOnly123!",
                ["JwtSettings:Issuer"] = "VNVTStore",
                ["JwtSettings:Audience"] = "VNVTStoreUsers",
                ["JwtSettings:ExpirationInMinutes"] = "60",
                ["RateLimiting:PermitLimit"] = "1000",
                ["RateLimiting:AuthPermitLimit"] = "1000"
            });
        });

        builder.ConfigureServices((context, services) =>
        {
            // Remove the production DbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Add Test DbContext using the overridden connection string
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql("Host=localhost;Database=VNVTStore_Test;Username=postgres;Password=password");
            });

            // Ensure migrations and seeding happen for the test DB (handled in Program.cs but we can do extra here if needed)
        });

        builder.UseEnvironment("Testing");
    }
}
