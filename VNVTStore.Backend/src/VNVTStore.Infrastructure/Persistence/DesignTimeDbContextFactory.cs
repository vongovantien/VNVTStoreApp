using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VNVTStore.Infrastructure.Persistence;

/// <summary>
/// Design-time DbContext Factory cho EF Core Migrations và Scaffolding
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Tìm đường dẫn đến appsettings.json trong API project
        var currentDir = Directory.GetCurrentDirectory();
        var basePath = Path.Combine(currentDir, "src", "VNVTStore.API");
        if (!Directory.Exists(basePath))
        {
             basePath = Path.Combine(currentDir, "..", "VNVTStore.API");
        }
        if (!Directory.Exists(basePath))
        {
             basePath = currentDir;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets("526b9549-5cbc-4fb6-9535-1fdcf3c7e6e4")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
