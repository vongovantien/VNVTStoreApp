using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblProductPromotion
{
    public string Code { get; set; } = null!;

    public string ProductCode { get; set; } = null!;

    public string PromotionCode { get; set; } = null!;

    public virtual TblProduct ProductCodeNavigation { get; set; } = null!;

    public virtual TblPromotion PromotionCodeNavigation { get; set; } = null!;
}
