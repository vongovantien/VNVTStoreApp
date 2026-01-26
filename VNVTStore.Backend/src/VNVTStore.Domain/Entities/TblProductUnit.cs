using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblProductUnit : IEntity
{
    public string Code { get; set; } = Guid.NewGuid().ToString("N");
    public string ProductCode { get; set; } = null!;
    public string UnitCode { get; set; } = null!; // FK to TblUnit (Catalog)
    public decimal ConversionRate { get; set; }
    public decimal Price { get; set; }
    public bool IsBaseUnit { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }

    public virtual TblProduct Product { get; set; } = null!;
    public virtual TblUnit Unit { get; set; } = null!;
}
