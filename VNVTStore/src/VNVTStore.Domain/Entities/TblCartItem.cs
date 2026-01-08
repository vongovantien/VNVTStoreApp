using System;
using System.Collections.Generic;

namespace VNVTStore.Infrastructure;

public partial class TblCartItem
{
    public string Code { get; set; } = null!;

    public string CartCode { get; set; } = null!;

    public string ProductCode { get; set; } = null!;

    public int Quantity { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual TblCart CartCodeNavigation { get; set; } = null!;

    public virtual TblProduct ProductCodeNavigation { get; set; } = null!;
}
