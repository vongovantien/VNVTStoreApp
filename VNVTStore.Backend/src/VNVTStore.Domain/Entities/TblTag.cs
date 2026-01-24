using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblTag : IEntity
{
    public string Code { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }

    public virtual ICollection<TblProductTag> TblProductTags { get; set; } = new List<TblProductTag>();
}
