using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Tests.Integration;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Create open SqliteConnection so schema stays in memory
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            services.AddSingleton<DbConnection>(connection);

            services.AddDbContext<ApplicationDbContext>((container, options) =>
            {
                var conn = container.GetRequiredService<DbConnection>();
                options.UseSqlite(conn);
            });

            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                
                db.Database.EnsureCreated();
                
                // Seed Roles
                if (!db.TblRoles.Any())
                {
                    db.TblRoles.AddRange(
                        new TblRole { Code = "ADMIN", Name = "Admin" },
                        new TblRole { Code = "CUSTOMER", Name = "Customer" }
                    );
                    db.SaveChanges();
                }
            }
        });
    }
}
