using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using VNVTStore.Infrastructure.Hubs;

namespace VNVTStore.Infrastructure.Extensions;

/// <summary>
/// Maps SignalR hubs. Keeps hub types inside Infrastructure so API does not reference them.
/// </summary>
public static class HubEndpointExtensions
{
    public static IEndpointRouteBuilder MapInfrastructureHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<NotificationHub>("/notificationHub");
        return endpoints;
    }
}
