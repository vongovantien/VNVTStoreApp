using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblDebtLog : IEntity
{
    public string Code { get; set; } = Guid.NewGuid().ToString("N");
    public string UserCode { get; set; } = null!;
    public string? OrderCode { get; set; } // Linked to an Order if applicable
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; } // Positive = Debt Increase, Negative = Payment
    
    public string Reason { get; set; } = null!; // "Order #123", "Payment via Bank"
    public decimal BalanceAfter { get; set; }
    
    public string? RecordedBy { get; set; } // Admin who recorded it
    
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }

    public virtual TblUser User { get; set; } = null!;
    public virtual TblOrder? Order { get; set; }
}
