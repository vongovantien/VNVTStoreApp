using System;
using System.Collections.Generic;

namespace VNVTStore.Infrastructure;

public partial class TblProductImage
{
    public string Code { get; set; } = null!;

    public string ProductCode { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string? AltText { get; set; }

    public bool? IsPrimary { get; set; }

    public int? SortOrder { get; set; }

    public virtual TblProduct ProductCodeNavigation { get; set; } = null!;
}
