using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblUser
{
    public string Code { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool IsActive { get; set; } = true;

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }

    public virtual ICollection<TblAddress> TblAddresses { get; set; } = new List<TblAddress>();

    public virtual ICollection<TblCart> TblCarts { get; set; } = new List<TblCart>();

    public virtual ICollection<TblOrder> TblOrders { get; set; } = new List<TblOrder>();

    public virtual ICollection<TblReview> TblReviews { get; set; } = new List<TblReview>();

    public virtual ICollection<TblQuote> TblQuotes { get; set; } = new List<TblQuote>();

}
