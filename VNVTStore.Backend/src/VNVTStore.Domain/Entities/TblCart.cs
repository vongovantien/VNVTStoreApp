using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblCart
{
    public string Code { get; set; } = null!;

    public string UserCode { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<TblCartItem> TblCartItems { get; set; } = new List<TblCartItem>();

    public virtual TblUser UserCodeNavigation { get; set; } = null!;
}
