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
    public int LoyaltyPoints { get; set; }
    public decimal DebtLimit { get; set; }
    public decimal CurrentDebt { get; set; }
    public bool TwoFactorEnabled { get; set; } = false;
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
    public string? RoleCode { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? RoleCode { get; set; }
    public bool? IsActive { get; set; }
    public string? Password { get; set; } // Optional: Admin resetting password
    public string? AvatarUrl { get; set; }
}

public class AuthResponseDto
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public UserDto? User { get; set; }
    public bool RequiresTwoFactor { get; set; } = false;
    public string? TwoFactorToken { get; set; }
}

public class TwoFactorSetupDto
{
    public string SecretKey { get; set; } = null!;
    public string QrCodeUri { get; set; } = null!;
    public string ManualEntryKey { get; set; } = null!;
}

public class TwoFactorEnableDto
{
    public List<string> RecoveryCodes { get; set; } = new();
}

public class VerifyTwoFactorRequest
{
    public string TwoFactorToken { get; set; } = null!;
    public string Code { get; set; } = null!;
}

public class EnableTwoFactorRequest
{
    public string Code { get; set; } = null!;
}

public class DisableTwoFactorRequest
{
    public string Password { get; set; } = null!;
    public string Code { get; set; } = null!;
}
