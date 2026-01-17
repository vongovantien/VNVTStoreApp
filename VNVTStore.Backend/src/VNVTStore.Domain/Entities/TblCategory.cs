using System;
using System.Collections.Generic;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblCategory : IEntity
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ParentCode { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public string? ModifiedType { get; set; }

    public virtual ICollection<TblCategory> InverseParentCodeNavigation { get; set; } = new List<TblCategory>();

    public virtual TblCategory? ParentCodeNavigation { get; set; }

    public virtual ICollection<TblProduct> TblProducts { get; set; } = new List<TblProduct>();
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
