using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblOrder
{
    private TblOrder() 
    {
        TblOrderItems = new List<TblOrderItem>();
    }

    public string Code { get; private set; } = null!;

    public string UserCode { get; private set; } = null!;

    public DateTime? OrderDate { get; private set; }

    public decimal TotalAmount { get; private set; }

    public decimal ShippingFee { get; private set; }

    public decimal? DiscountAmount { get; private set; }

    public decimal FinalAmount { get; private set; }

    public string? Status { get; private set; }

    public string? AddressCode { get; private set; }

    public string? CouponCode { get; private set; }

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
        var finalAmount = totalAmount + shippingFee - (discountAmount ?? 0);
        if (finalAmount < 0) finalAmount = 0;

        return new TblOrder
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            UserCode = userCode,
            AddressCode = addressCode,
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            ShippingFee = shippingFee,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            Status = "Pending",
            CouponCode = couponCode,
            TblOrderItems = new List<TblOrderItem>()
        };
    }

    public void AddOrderItem(TblOrderItem item)
    {
        TblOrderItems.Add(item);
    }

    public void UpdateStatus(string status)
    {
        // Add valid status transitions if needed
        Status = status;
    }

    public void Cancel(string reason)
    {
        if (Status == "Completed" || Status == "Cancelled")
        {
             throw new InvalidOperationException($"Cannot cancel order in status {Status}.");
        }
        Status = "Cancelled";
        // Optionally store cancellation reason if there was a field
    }
}
