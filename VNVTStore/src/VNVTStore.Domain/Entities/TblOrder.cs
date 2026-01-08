using System;
using System.Collections.Generic;

namespace VNVTStore.Infrastructure;

public partial class TblOrder
{
    public string Code { get; set; } = null!;

    public string UserCode { get; set; } = null!;

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public string? Status { get; set; }

    public string? AddressCode { get; set; }

    public string? CouponCode { get; set; }

    public virtual TblAddress? AddressCodeNavigation { get; set; }

    public virtual TblCoupon? CouponCodeNavigation { get; set; }

    public virtual ICollection<TblOrderItem> TblOrderItems { get; set; } = new List<TblOrderItem>();

    public virtual TblPayment? TblPayment { get; set; }

    public virtual TblUser UserCodeNavigation { get; set; } = null!;
}
