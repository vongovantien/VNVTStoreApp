using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace VNVTStore.Domain.Entities;

public partial class TblUser : IEntity
{
    private TblUser() 
    {
        // Required for EF Core
        TblAddresses = new List<TblAddress>();
        TblCarts = new List<TblCart>();
        TblOrders = new List<TblOrder>();
        TblReviews = new List<TblReview>();
        TblQuotes = new List<TblQuote>();
        TblUserLogins = new List<TblUserLogin>();
    }

    public string Code { get; set; } = null!;

    public string Username { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public string? FullName { get; private set; }

    public string? Phone { get; private set; }

    public string? RoleCode { get; private set; }
    public virtual TblRole? RoleCodeNavigation { get; private set; }

    public UserRole Role { get; private set; } // Keep for compatibility temporarily

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLogin { get; private set; }

    public bool IsActive { get; set; } = true;

    public string? ModifiedType { get; set; }

    public string? RefreshToken { get; private set; }

    public DateTime? RefreshTokenExpiryTime { get; private set; }

    [Column("LoyaltyPoints")]
    public int LoyaltyPoints { get; private set; } = 0;

    public string? EmailVerificationToken { get; private set; }

    public bool IsEmailVerified { get; private set; } = false;

    public string? PasswordResetToken { get; private set; }

    public DateTime? ResetTokenExpiry { get; private set; }

    public string? AvatarUrl { get; private set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DebtLimit { get; private set; } = 0; // Maximum allowed debt

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentDebt { get; private set; } = 0; // Current outstanding debt

    public virtual ICollection<TblAddress> TblAddresses { get; private set; }
    public virtual ICollection<TblUserLogin> TblUserLogins { get; private set; }

    // ... (Keep other collections)

    // Factory method to create a new user (Rich Domain Model)
    // ... (Keep Create method)
    
    // ...

    public void AddLogin(string loginProvider, string providerKey, string? displayName)
    {
        var existing = TblUserLogins.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
        if (existing == null)
        {
            TblUserLogins.Add(new TblUserLogin 
            { 
                LoginProvider = loginProvider, 
                ProviderKey = providerKey, 
                ProviderDisplayName = displayName,
                UserId = this.Code 
            });
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public static TblUser CreateExternal(string email, string? fullName, string loginProvider, string providerKey, string? providerDisplayName, string? avatarUrl = null)
    {
         if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
         
         var user = new TblUser
         {
             Code = Guid.NewGuid().ToString("N").Substring(0, 10),
             Username = email, 
             Email = email,
             FullName = fullName,
             Role = UserRole.Customer,
             IsActive = true,
             CreatedAt = DateTime.UtcNow,
             UpdatedAt = DateTime.UtcNow,
             IsEmailVerified = true, 
             PasswordHash = "",
             AvatarUrl = avatarUrl,
             RoleCode = "CUSTOMER" // Default role code
         };
         
         user.AddLogin(loginProvider, providerKey, providerDisplayName);
         
         return user;
    }
    
    // ...

    public virtual ICollection<TblCart> TblCarts { get; private set; }

    public virtual ICollection<TblOrder> TblOrders { get; private set; }

    public virtual ICollection<TblReview> TblReviews { get; private set; }

    public virtual ICollection<TblQuote> TblQuotes { get; private set; }

    // Factory method to create a new user (Rich Domain Model)
    public static TblUser Create(string username, string email, string passwordHash, string? fullName, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty", nameof(username));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        var user = new TblUser
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsEmailVerified = false,
            EmailVerificationToken = Guid.NewGuid().ToString("N"),
            RoleCode = role == UserRole.Admin ? "ADMIN" : "CUSTOMER"
        };
        
        return user;
    }
    
    public void UpdateProfile(string? fullName, string? phone, string? email, string? avatarUrl = null)
    {
        FullName = fullName ?? FullName;
        Phone = phone ?? Phone;
        if (!string.IsNullOrWhiteSpace(avatarUrl))
        {
            AvatarUrl = avatarUrl;
        }
        // Business rule: Email change might require verification or check, but for now we allow update.
        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = email;
        }
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateAvatar(string avatarUrl)
    {
         if (!string.IsNullOrWhiteSpace(avatarUrl))
         {
             AvatarUrl = avatarUrl;
             UpdatedAt = DateTime.UtcNow;
         }
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
    }

    public void UpdateRole(string roleCode)
    {
        RoleCode = roleCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRoleEnum(UserRole role)
    {
        Role = role;
        RoleCode = role == UserRole.Admin ? "ADMIN" : "CUSTOMER";
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiryTime = expiry;
    }

    public void AddLoyaltyPoints(int points)
    {
        if (points < 0) throw new ArgumentException("Points cannot be negative", nameof(points));
        LoyaltyPoints += points;
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyEmail(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token cannot be empty", nameof(token));
        if (EmailVerificationToken != token) throw new InvalidOperationException("Invalid verification token");
        
        IsEmailVerified = true;
        EmailVerificationToken = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void GeneratePasswordResetToken()
    {
        PasswordResetToken = Guid.NewGuid().ToString("N");
        ResetTokenExpiry = DateTime.UtcNow.AddHours(24);
        UpdatedAt = DateTime.UtcNow;
    }



    public void ResetPassword(string token, string passwordHash)
    {
        if (PasswordResetToken != token) throw new InvalidOperationException("Invalid reset token");
        if (ResetTokenExpiry < DateTime.UtcNow) throw new InvalidOperationException("Reset token expired");
        
        PasswordHash = passwordHash;
        PasswordResetToken = null;
        ResetTokenExpiry = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDebt(decimal amount)
    {
        CurrentDebt += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDebtLimit(decimal limit)
    {
        DebtLimit = limit;
        UpdatedAt = DateTime.UtcNow;
    }
}
