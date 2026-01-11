using Microsoft.Extensions.DependencyInjection;
using VNVTStore.Application.MappingProfiles;

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

        // Add Localization Service

        return services;
    }
}


