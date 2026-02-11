namespace VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Attributes;

public class UserDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public string? RoleCode { get; set; }
    [Reference("TblRole", "RoleCode", "Name")]
    public string? RoleName { get; set; }
    public bool IsActive { get; set; } // Ensure IsActive is exposed
    public bool IsEmailVerified { get; set; } = false;
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public string? Token { get; set; }
    public string? Avatar { get; set; }
    public List<string> Permissions { get; set; } = new();
    public List<string> Menus { get; set; } = new();
}

public class CreateUserDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? Password { get; set; } // Optional: Admin resetting password
    public string? AvatarUrl { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}
