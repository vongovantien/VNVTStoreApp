using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblUnit : IEntity
{
    public string Code { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = null!; // "Cuộn", "Thùng", "Mét"
    public bool IsActive { get; set; } = true;
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }

    public virtual ICollection<TblProductUnit> TblProductUnits { get; set; } = new List<TblProductUnit>();
}
