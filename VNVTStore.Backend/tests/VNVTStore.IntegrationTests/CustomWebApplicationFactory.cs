using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.IntegrationTests;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Explicitly add user secrets for the TProgram project (the API)
            config.AddUserSecrets<TProgram>(optional: true);
            
            // Use existing configuration to get the real connection string, then replace the DB name
            var builtConfig = config.Build();
            var originalConnectionString = builtConfig.GetConnectionString("DefaultConnection");
            
            string testConnectionString;
            // Debug: Log the connection string (masked)
            Console.WriteLine($"[DEBUG] Original Connection String found: {(!string.IsNullOrEmpty(originalConnectionString))}");
            if (!string.IsNullOrEmpty(originalConnectionString))
            {
                // Replace database name for testing
                // This is a simple replacement, might need adjustment if DB name appears elsewhere in string
                testConnectionString = originalConnectionString.Replace("shoppingdb", "VNVTStore_Test_v3");
                if (testConnectionString == originalConnectionString)
                {
                     // Fallback if 'shoppingdb' wasn't there
                     testConnectionString = originalConnectionString.Replace("vnvtstore", "VNVTStore_Test_v3");
                }
            }
            else
            {
                // Absolute fallback if no connection string found
                testConnectionString = "Host=localhost;Database=VNVTStore_Test_v3;Username=postgres;Password=password";
            }

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = testConnectionString,
                ["JwtSettings:SecretKey"] = "SuperSecretKeyForIntegrationTestingOnly1234567890!",
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

            // Add Test DbContext using the overridden connection string from config (which now includes secrets)
            var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            // Ensure the test database is created and seeded
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
                
                // Explicitly seed data for the test DB
                var passwordHasher = scopedServices.GetRequiredService<IPasswordHasher>();
                VNVTStore.Application.Seeding.DataSeeder.SeedAsync(db, passwordHasher).Wait();
                
                var userCount = db.TblUsers.Count();
                Console.WriteLine($"[DEBUG] After Seeding: User Count = {userCount}");
                var adminUser = db.TblUsers.FirstOrDefault(u => u.Username == "admin");
                Console.WriteLine($"[DEBUG] Admin user exists: {adminUser != null}");
            }
        });
      // Ensure migrations and seeding happen for the test DB (handled in Program.cs but we can do extra here if needed)
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddScoped<IImageUploadService, MockImageUploadService>();
        });
    }

    public class MockImageUploadService : IImageUploadService
    {
        private readonly IApplicationDbContext _context;
        public MockImageUploadService(IApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Result<FileDto>> UploadImageAsync(Stream imageStream, string fileName, string folder = "products")
            => Task.FromResult(Result.Success(new FileDto { Code = "test", Url = "test" }));

        public Task<Result<IEnumerable<FileDto>>> UploadImagesAsync(IEnumerable<(Stream Stream, string FileName)> images, string folder = "products")
            => Task.FromResult(Result.Success(Enumerable.Empty<FileDto>()));

        public Task<Result<FileDto>> UploadBase64Async(string base64Content, string fileName, string folder = "products")
            => Task.FromResult(Result.Success(new FileDto { Code = "test", Url = "test" }));

        public Task<Result<IEnumerable<FileDto>>> UploadBase64ImagesAsync(IEnumerable<(string Base64Content, string FileName)> images, string folder = "products")
            => Task.FromResult(Result.Success(Enumerable.Empty<FileDto>()));

        public async Task<Result<FileDto>> UploadUrlAsync(string url, string fileName, string folder = "products")
        {
            var code = $"FIL{DateTime.Now.Ticks}";
            var file = VNVTStore.Domain.Entities.TblFile.Create(fileName, fileName, ".jpg", "image/jpeg", 1024, url);
            file.Code = code;
            _context.TblFiles.Add(file);
            await _context.SaveChangesAsync(default);
            return Result.Success(new FileDto { Code = code, Url = url, Path = url });
        }

        public Task<Result> DeleteImageAsync(string imageUrl) => Task.FromResult(Result.Success());
        public Task<Result> DeleteImagesAsync(IEnumerable<string> imageUrls) => Task.FromResult(Result.Success());
    }
}
