namespace VNVTStore.Application.Interfaces;

/// <summary>
/// JWT service interface — uses RoleCode (string) as source of truth instead of the legacy UserRole enum.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate a signed JWT access token.
    /// </summary>
    /// <param name="userCode">User primary key.</param>
    /// <param name="username">Display username.</param>
    /// <param name="email">User email.</param>
    /// <param name="roleCode">RBAC role code, e.g. "ADMIN", "CUSTOMER", "POS_STAFF".</param>
    /// <param name="permissions">List of permission names granted to this role.</param>
    /// <param name="menus">List of menu codes visible to this role.</param>
    string GenerateToken(string userCode, string username, string email, string roleCode,
        IEnumerable<string> permissions, IEnumerable<string> menus);

    string GenerateRefreshToken();

    System.Security.Claims.ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
