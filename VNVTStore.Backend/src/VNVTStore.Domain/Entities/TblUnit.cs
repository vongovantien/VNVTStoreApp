using System.Collections.Generic;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblUnit : BaseEntity
{
    public string Name { get; set; } = null!; // "Cuộn", "Thùng", "Mét"

    public virtual ICollection<TblProductUnit> TblProductUnits { get; set; } = new List<TblProductUnit>();
}
