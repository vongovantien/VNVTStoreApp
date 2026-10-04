using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserCode => _httpContextAccessor.HttpContext?.User?.FindFirstValue("userCode")
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    // RoleCode is now uppercase ("ADMIN", "CUSTOMER", "POS_STAFF")
    public bool IsAdmin => string.Equals(Role, "ADMIN", StringComparison.OrdinalIgnoreCase);
}
