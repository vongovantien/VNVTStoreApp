using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public partial class TblCoupon : BaseEntity
{
    public string? PromotionCode { get; set; }

    public int? UsageCount { get; set; }

    // No IsFixed/CreatedBy/UpdatedBy columns for TblCoupon in DB yet
    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    public virtual TblPromotion? PromotionCodeNavigation { get; set; }

    public virtual ICollection<TblOrder> TblOrders { get; set; } = new List<TblOrder>();
}
