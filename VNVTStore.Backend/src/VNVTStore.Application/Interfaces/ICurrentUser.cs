namespace VNVTStore.Application.Interfaces;

/// <summary>
/// Current user interface - dùng string UserCode thay vì int UserId
/// </summary>
public interface ICurrentUser
{
    string? UserCode { get; }
    string? Username { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
