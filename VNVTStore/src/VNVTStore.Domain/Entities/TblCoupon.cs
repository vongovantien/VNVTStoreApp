using System;
using System.Collections.Generic;

namespace VNVTStore.Infrastructure;

public partial class TblCoupon
{
    public string Code { get; set; } = null!;

    public string? PromotionCode { get; set; }

    public int? UsageCount { get; set; }

    public virtual TblPromotion? PromotionCodeNavigation { get; set; }

    public virtual ICollection<TblOrder> TblOrders { get; set; } = new List<TblOrder>();
}
