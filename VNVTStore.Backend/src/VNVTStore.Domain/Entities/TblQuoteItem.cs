using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblQuoteItem : IEntity
{
    public string Code { get; set; } = Guid.NewGuid().ToString("N");
    public string QuoteCode { get; set; } = null!;
    public string ProductCode { get; set; } = null!;
    public string? UnitCode { get; set; } // Specific unit (Box, Roll, etc.)
    public int Quantity { get; set; }
    
    public decimal RequestPrice { get; set; } // Price user/system originally asked
    public decimal ApprovedPrice { get; set; } // Price admin approves
    public decimal TotalLineAmount => ApprovedPrice * Quantity;

    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }

    public virtual TblQuote Quote { get; set; } = null!;
    public virtual TblProduct Product { get; set; } = null!;
    public virtual TblUnit? Unit { get; set; }
}
