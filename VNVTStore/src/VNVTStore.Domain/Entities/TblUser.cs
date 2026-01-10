using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblUser
{
    private TblUser() 
    {
        // Required for EF Core
        TblAddresses = new List<TblAddress>();
        TblCarts = new List<TblCart>();
        TblOrders = new List<TblOrder>();
        TblReviews = new List<TblReview>();
        TblQuotes = new List<TblQuote>();
    }

    public string Code { get; private set; } = null!;

    public string Username { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public string? FullName { get; private set; }

    public string? Phone { get; private set; }

    public string? Role { get; private set; }

    public DateTime? CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public DateTime? LastLogin { get; private set; }

    public bool IsActive { get; private set; } = true;

    public string? RefreshToken { get; private set; }

    public DateTime? RefreshTokenExpiry { get; private set; }

    public virtual ICollection<TblAddress> TblAddresses { get; private set; }

    public virtual ICollection<TblCart> TblCarts { get; private set; }

    public virtual ICollection<TblOrder> TblOrders { get; private set; }

    public virtual ICollection<TblReview> TblReviews { get; private set; }

    public virtual ICollection<TblQuote> TblQuotes { get; private set; }

    // Factory method to create a new user (Rich Domain Model)
    public static TblUser Create(string username, string email, string passwordHash, string? fullName, string role)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty", nameof(username));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        var user = new TblUser
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        
        return user;
    }
    
    public void UpdateProfile(string fullName, string phone, string email)
    {
        FullName = fullName;
        Phone = phone;
        // Business rule: Email change might require verification or check, but for now we allow update.
        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = email;
        }
        UpdatedAt = DateTime.Now;
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.Now;
    }

    public void UpdateLastLogin()
    {
        LastLogin = DateTime.Now;
    }

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }

}
