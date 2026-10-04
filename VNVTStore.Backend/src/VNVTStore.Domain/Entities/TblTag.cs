using System.Collections.Generic;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblTag : BaseEntity
{
    public string Name { get; set; } = null!;

    public virtual ICollection<TblProductTag> TblProductTags { get; set; } = new List<TblProductTag>();
}
