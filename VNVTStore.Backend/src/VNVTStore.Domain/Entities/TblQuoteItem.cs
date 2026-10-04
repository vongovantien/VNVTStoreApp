using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblQuoteItem : BaseEntity
{
    public string QuoteCode { get; set; } = null!;
    public string ProductCode { get; set; } = null!;
    public string? UnitCode { get; set; } // Specific unit (Box, Roll, etc.)
    public int Quantity { get; set; }
    
    public decimal RequestPrice { get; set; } // Price user/system originally asked
    public decimal ApprovedPrice { get; set; } // Price admin approves
    public decimal TotalLineAmount => ApprovedPrice * Quantity;

    public virtual TblQuote Quote { get; set; } = null!;
    public virtual TblProduct Product { get; set; } = null!;
    public virtual TblUnit? Unit { get; set; }
}
