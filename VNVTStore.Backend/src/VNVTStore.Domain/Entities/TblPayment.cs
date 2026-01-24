using System;
using System.Collections.Generic;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblPayment : IEntity
{
    private TblPayment() { }

    public string Code { get; set; } = null!;

    public string OrderCode { get; private set; } = null!;

    public DateTime? PaymentDate { get; private set; }
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ModifiedType { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal Amount { get; private set; }

    public PaymentMethod Method { get; private set; }

    public string? TransactionId { get; private set; }

    public PaymentStatus Status { get; private set; }

    public virtual TblOrder OrderCodeNavigation { get; private set; } = null!;

    public static TblPayment Create(string orderCode, decimal amount, PaymentMethod method)
    {
        return new TblPayment
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            OrderCode = orderCode,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending,
            PaymentDate = DateTime.UtcNow
        };
    }

    public void UpdateStatus(PaymentStatus status, string? transactionId = null)
    {
        Status = status;
        if (!string.IsNullOrEmpty(transactionId))
        {
            TransactionId = transactionId;
        }
    }
}
