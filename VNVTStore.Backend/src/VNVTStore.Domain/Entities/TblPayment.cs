using System;
using System.Collections.Generic;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public partial class TblPayment : BaseEntity
{
    private TblPayment() { }

    public string OrderCode { get; private set; } = null!;

    public DateTime? PaymentDate { get; private set; }

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

    /// <summary>
    /// Reset a non-completed payment so the customer can try again (possibly with another method).
    /// </summary>
    public void Retry(PaymentMethod method, decimal amount)
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Payment has already been completed.");

        Method = method;
        Amount = amount;
        Status = PaymentStatus.Pending;
        PaymentDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCompleted(string? gatewayTransactionId)
    {
        Status = PaymentStatus.Completed;
        if (!string.IsNullOrEmpty(gatewayTransactionId)) TransactionId = gatewayTransactionId;
        PaymentDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string? gatewayTransactionId)
    {
        if (Status == PaymentStatus.Completed) return;
        Status = PaymentStatus.Failed;
        if (!string.IsNullOrEmpty(gatewayTransactionId)) TransactionId = gatewayTransactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOnlineGateway => Method is PaymentMethod.VnPay or PaymentMethod.MoMo;
}
