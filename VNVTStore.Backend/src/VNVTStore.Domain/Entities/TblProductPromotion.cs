using System;
using System.Collections.Generic;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblProductPromotion : IEntity
{
    public string Code { get; set; } = null!;

    public string ProductCode { get; set; } = null!;

    public string PromotionCode { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ModifiedType { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual TblProduct ProductCodeNavigation { get; set; } = null!;

    public virtual TblPromotion PromotionCodeNavigation { get; set; } = null!;
}
