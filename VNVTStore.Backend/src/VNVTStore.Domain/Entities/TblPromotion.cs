using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblPromotion
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int? UsageLimit { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TblCoupon> TblCoupons { get; set; } = new List<TblCoupon>();

    public virtual ICollection<TblProductPromotion> TblProductPromotions { get; set; } = new List<TblProductPromotion>();
}
