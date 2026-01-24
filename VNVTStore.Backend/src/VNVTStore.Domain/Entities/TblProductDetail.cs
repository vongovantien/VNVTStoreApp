using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Domain.Entities;

public class TblProductDetail : IEntity
{
    public string Code { get; set; } = Guid.NewGuid().ToString("N");
    public string ProductCode { get; set; } = null!;
    [Column("DetailType")]
    public ProductDetailType DetailType { get; set; } = ProductDetailType.Spec; // SPEC, LOGISTICS, RELATION, IMAGE
    public string SpecName { get; set; } = null!;
    public string SpecValue { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }

    public virtual TblProduct Product { get; set; } = null!;
}
