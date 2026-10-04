using System.Collections.Generic;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblBrand : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }

    public virtual ICollection<TblProduct> TblProducts { get; set; } = new List<TblProduct>();
}
