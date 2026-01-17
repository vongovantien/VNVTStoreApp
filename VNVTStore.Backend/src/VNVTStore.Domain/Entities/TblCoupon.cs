using System;
using System.Collections.Generic;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblCoupon : IEntity
{
    public string Code { get; set; } = null!;

    public string? PromotionCode { get; set; }

    public int? UsageCount { get; set; }

    public bool IsActive { get; set; } = true;

    public string? ModifiedType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TblPromotion? PromotionCodeNavigation { get; set; }

    public virtual ICollection<TblOrder> TblOrders { get; set; } = new List<TblOrder>();
}
