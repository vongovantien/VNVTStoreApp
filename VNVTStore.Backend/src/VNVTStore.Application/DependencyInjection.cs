using Microsoft.Extensions.DependencyInjection;
using VNVTStore.Application.MappingProfiles;
using VNVTStore.Application.Strategies;
using VNVTStore.Domain.Strategies;

namespace VNVTStore.Application;

/// <summary>
/// Dependency Injection cho Application Layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // Add MediatR - tự động register tất cả handlers trong Assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Registry Strategies
        services.AddSingleton<IShippingStrategy, StandardShippingStrategy>();

        // Register Data Seeder
        services.AddScoped<VNVTStore.Application.Seeding.DataSeeder>();

        return services;
    }
}


