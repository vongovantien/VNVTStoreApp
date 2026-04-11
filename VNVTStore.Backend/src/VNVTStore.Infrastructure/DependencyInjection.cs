using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Infrastructure.Persistence.Repositories;
using VNVTStore.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using VNVTStore.Infrastructure.Authorization;

namespace VNVTStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors());

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());




        // Add Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Add Dapper Context
        services.AddSingleton<IDapperContext, DapperContext>();

        // Add Services
        services.AddScoped<ISecretConfigurationService, SecretConfigurationService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        // Ideally we pass IWebHostEnvironment but for now checking config or defaulting
        var useMockImage = configuration.GetValue<bool>("ImageSettings:UseMock", false);
        
        if (useMockImage)
        {
            services.AddScoped<IImageUploadService, MockImageUploadService>();
        }
        else
        {
            services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();
        }
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddTransient<INotificationService, NotificationService>();
        services.AddScoped<ILoyaltyService, LoyaltyService>();
        services.AddScoped<IPromotionEngine, PromotionEngine>();
        
        // Add Caching - Use Redis if configured, otherwise Memory cache
        services.AddMemoryCache();
        var redisConnection = configuration.GetValue<string>("CacheSettings:Redis:ConnectionString");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }
        // Ideally we pass IWebHostEnvironment but for now checking config or defaulting
        var useMockEmail = configuration.GetValue<bool>("EmailSettings:UseMock", false);
        if (useMockEmail)
        {
            services.AddTransient<IEmailService, MockEmailService>();
        }
        else
        {
            services.AddTransient<IEmailService, EmailService>();
        }

        services.AddScoped<IBaseUrlService, BaseUrlService>();
        services.AddHttpContextAccessor();

        // RBAC Authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();
        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
             allowedOrigins = new[] { 
                "http://localhost:5173", 
                "http://localhost:5174", 
                "http://localhost:5175", 
                "http://localhost:5176",
                "https://scalar.com"
             };
        }
        // Add JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Autonomous Logic Hubs (Phase 9)
        services.AddHostedService<LogicHubWorker>();

        return services;
    }
}
