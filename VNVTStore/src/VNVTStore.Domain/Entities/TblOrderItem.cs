using System;
using System.Collections.Generic;

namespace VNVTStore.Infrastructure;

public partial class TblOrderItem
{
    public string Code { get; set; } = null!;

    public string OrderCode { get; set; } = null!;

    public string? ProductCode { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtOrder { get; set; }

    public decimal? DiscountAmount { get; set; }

    public virtual TblOrder OrderCodeNavigation { get; set; } = null!;

    public virtual TblProduct? ProductCodeNavigation { get; set; }

    public virtual ICollection<TblReview> TblReviews { get; set; } = new List<TblReview>();
}
