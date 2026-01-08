using System;
using System.Collections.Generic;

namespace VNVTStore.Infrastructure;

public partial class TblCategory
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ParentCode { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TblCategory> InverseParentCodeNavigation { get; set; } = new List<TblCategory>();

    public virtual TblCategory? ParentCodeNavigation { get; set; }

    public virtual ICollection<TblProduct> TblProducts { get; set; } = new List<TblProduct>();
}
