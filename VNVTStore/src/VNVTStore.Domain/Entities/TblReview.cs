using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblReview
{
    public string Code { get; set; } = null!;

    public string? OrderItemCode { get; set; }

    public string UserCode { get; set; } = null!;

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsApproved { get; set; }

    public virtual TblOrderItem? OrderItemCodeNavigation { get; set; }

    public virtual TblUser UserCodeNavigation { get; set; } = null!;
}
