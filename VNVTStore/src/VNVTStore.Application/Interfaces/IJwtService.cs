namespace VNVTStore.Application.Interfaces;

/// <summary>
/// JWT service interface - dùng string userCode thay vì int userId
/// </summary>
public interface IJwtService
{
    string GenerateToken(string userCode, string username, string email, string? role);
}
