using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Infrastructure.Persistence.Repositories;
using VNVTStore.Infrastructure.Services;

namespace VNVTStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());




        // Add Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Add Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IImageUploadService, LocalImageUploadService>();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddHttpContextAccessor();

        // Add JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        return services;
    }
}
