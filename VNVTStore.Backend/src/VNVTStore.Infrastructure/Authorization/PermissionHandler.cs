using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using VNVTStore.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace VNVTStore.Infrastructure.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PermissionHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userCode = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userCode))
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Check if user has a role that contains the permission
        var hasPermission = await dbContext.TblUsers
            .Where(u => u.Code == userCode && u.IsActive)
            .AnyAsync(u => u.RoleCodeNavigation != null && 
                           u.RoleCodeNavigation.IsActive &&
                           u.RoleCodeNavigation.TblRolePermissions.Any(rp => rp.PermissionCodeNavigation.Name == requirement.Permission));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
