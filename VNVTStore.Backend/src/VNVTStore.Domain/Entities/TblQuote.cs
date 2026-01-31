using System;
using System.Collections.Generic;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblQuote : IEntity
{
    public string Code { get; set; } = null!;

    public string? UserCode { get; set; }
    
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    // Removed single item fields (ProductCode, Quantity, QuotedPrice)
    
    public decimal TotalAmount { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ReferenceNumber { get; set; }

    public string? Note { get; set; }
    public string? AdminNote { get; set; }

    public string Status { get; set; } = null!; // Pending, Approved, Rejected, ConvertedToOrder

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ModifiedType { get; set; }

    public virtual ICollection<TblQuoteItem> TblQuoteItems { get; set; } = new List<TblQuoteItem>();
    public virtual TblUser? UserCodeNavigation { get; set; }
}
