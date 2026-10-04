using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public partial class TblCategory : BaseEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ParentCode { get; set; }

    public virtual ICollection<TblCategory> InverseParentCodeNavigation { get; set; } = new List<TblCategory>();

    public virtual TblCategory? ParentCodeNavigation { get; set; }

    public virtual ICollection<TblProduct> TblProducts { get; set; } = new List<TblProduct>();
}
