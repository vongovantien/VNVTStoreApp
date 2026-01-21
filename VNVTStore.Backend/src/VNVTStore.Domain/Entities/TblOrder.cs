using System;
using System.Collections.Generic;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblOrder : IEntity
{
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private TblOrder() 
    {
        TblOrderItems = new List<TblOrderItem>();
    }

    public string Code { get; set; } = null!;

    public string UserCode { get; set; } = null!;

    public string? ModifiedType { get; set; }

    public DateTime? OrderDate { get; private set; }

    public decimal TotalAmount { get; private set; }

    public decimal ShippingFee { get; private set; }

    public decimal? DiscountAmount { get; private set; }

    public decimal FinalAmount { get; private set; }

    public OrderStatus Status { get; private set; }

    public string? AddressCode { get; private set; }

    public string? CouponCode { get; private set; }
    
    public string? VerificationToken { get; private set; }
    
    public DateTime? VerificationTokenExpiresAt { get; private set; }

    public virtual TblAddress? AddressCodeNavigation { get; private set; }

    public virtual TblCoupon? CouponCodeNavigation { get; private set; }

    public virtual ICollection<TblOrderItem> TblOrderItems { get; private set; }

    public virtual TblPayment? TblPayment { get; private set; }

    public virtual TblUser UserCodeNavigation { get; private set; } = null!;

    public static TblOrder Create(
        string userCode, 
        string addressCode, 
        decimal totalAmount, 
        decimal shippingFee, 
        decimal? discountAmount,
        string? couponCode)
    {
        if (totalAmount < 0) throw new ArgumentException("Total amount cannot be negative.", nameof(totalAmount));
        if (shippingFee < 0) throw new ArgumentException("Shipping fee cannot be negative.", nameof(shippingFee));

        var finalAmount = totalAmount + shippingFee - (discountAmount ?? 0);
        if (finalAmount < 0) finalAmount = 0;

        return new TblOrder
        {
            UserCode = userCode,
            AddressCode = addressCode,
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            ShippingFee = shippingFee,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            Status = OrderStatus.Pending,
            CouponCode = couponCode,
            TblOrderItems = new List<TblOrderItem>()
        };
    }

    public void AddOrderItem(TblOrderItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        TblOrderItems.Add(item);
    }

    public void UpdateStatus(OrderStatus status)
    {
        // Simple status transition rules
        if (Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
             throw new InvalidOperationException("Cannot change status of a cancelled order.");
             
        Status = status;
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
        {
             throw new InvalidOperationException($"Cannot cancel order in status {Status}.");
        }
        Status = OrderStatus.Cancelled;
    }

    public void SetVerificationToken(string token, DateTime expiry)
    {
        VerificationToken = token;
        VerificationTokenExpiresAt = expiry;
    }
}
