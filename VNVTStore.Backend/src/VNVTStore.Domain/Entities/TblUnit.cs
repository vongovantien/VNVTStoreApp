using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblUnit : IEntity
{
    public string Code { get; set; } = Guid.NewGuid().ToString("N");
    public string ProductCode { get; set; } = null!;
    public string UnitName { get; set; } = null!;     // "Cuộn", "Thùng"
    public decimal ConversionRate { get; set; } // 100
    public decimal Price { get; set; }       // Price for this unit
    public bool IsActive { get; set; } = true;
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }

    public virtual TblProduct Product { get; set; } = null!;
}
