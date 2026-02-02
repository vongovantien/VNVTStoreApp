using Microsoft.AspNetCore.Authorization;

namespace VNVTStore.Application.Authorization;

/// <summary>
/// Declares that an endpoint requires the given permission (policy name).
/// Implementation (policy resolution and checking) lives in Infrastructure.
/// </summary>
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}
