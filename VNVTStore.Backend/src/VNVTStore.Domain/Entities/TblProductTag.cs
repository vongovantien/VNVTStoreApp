using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblProductTag : IEntity
{
    public string ProductCode { get; set; } = null!;
    public string TagCode { get; set; } = null!;

    public string Code { get; set; } = Guid.NewGuid().ToString("N");
    public bool IsActive { get; set; } = true;
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }

    public virtual TblProduct Product { get; set; } = null!;
    public virtual TblTag Tag { get; set; } = null!;
}
