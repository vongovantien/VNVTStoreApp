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
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=VNVTStore_Test;Username=postgres;Password=password"
            });
            config.AddJsonFile("appsettings.json")
                  .AddEnvironmentVariables();
        });

        builder.ConfigureServices((context, services) =>
        {
            // The DB context is already added in Infrastructure.
            // Connection string is overridden in ConfigureAppConfiguration.
            // Program.cs handles migrations and seeding during host initialization.
        });

        builder.UseEnvironment("Development");
    }
}
