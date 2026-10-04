using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblDebtLog : BaseEntity
{
    public string UserCode { get; set; } = null!;
    public string? OrderCode { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; } // Positive = Debt Increase, Negative = Payment
    
    public string Reason { get; set; } = null!; // "Order #123", "Payment via Bank"
    public decimal BalanceAfter { get; set; }
    
    public string? RecordedBy { get; set; }

    // No IsFixed in DB for this table yet

    public virtual TblUser User { get; set; } = null!;
    public virtual TblOrder? Order { get; set; }
}
