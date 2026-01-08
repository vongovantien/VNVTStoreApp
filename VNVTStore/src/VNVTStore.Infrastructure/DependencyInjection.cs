using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Infrastructure.Persistence.Repositories;
using VNVTStore.Infrastructure.Services;

namespace VNVTStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Add Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Add Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        // Add JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        return services;
    }
}
