using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Interfaces;

/// <summary>
/// JWT service interface - dùng string userCode thay vì int userId
/// </summary>
public interface IJwtService
{
    string GenerateToken(string userCode, string username, string email, UserRole role, IEnumerable<string> permissions, IEnumerable<string> menus);
    string GenerateRefreshToken();
    System.Security.Claims.ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
