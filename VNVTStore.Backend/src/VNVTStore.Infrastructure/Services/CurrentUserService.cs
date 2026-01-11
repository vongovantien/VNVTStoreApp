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

    public string? UserCode => _httpContextAccessor.HttpContext?.User?.FindFirstValue("code") ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    // Note: Adjusted to look for "code" claim or NameIdentifier. DTO says Code.
    // In JwtService, I should check what claim is used for ID/Code.
    // Usually "id" or "sub" or ClaimTypes.NameIdentifier.
    // I'll assume NameIdentifier is the Code (string).

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => Role == "admin"; // Simple check
}
