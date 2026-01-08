using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblQuote
{
    public string Code { get; set; } = null!;

    public string UserCode { get; set; } = null!;

    public string ProductCode { get; set; } = null!;

    public int Quantity { get; set; }

    public string? Note { get; set; }

    public decimal? QuotedPrice { get; set; }

    public string? AdminNote { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TblProduct ProductCodeNavigation { get; set; } = null!;

    public virtual TblUser UserCodeNavigation { get; set; } = null!;
}
