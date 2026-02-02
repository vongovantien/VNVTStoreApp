using Microsoft.AspNetCore.Authorization;

namespace VNVTStore.Application.Authorization;

/// <summary>
/// Authorization requirement used by the policy provider and handler.
/// Handler implementation lives in Infrastructure.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
