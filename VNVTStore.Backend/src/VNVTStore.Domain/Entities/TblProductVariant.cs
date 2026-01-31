using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblProductVariant : IEntity
{
    public string Code { get; set; } = Guid.NewGuid().ToString("N");
    public string ProductCode { get; set; } = null!;
    public string SKU { get; set; } = null!;
    
    /// <summary>
    /// JSON string storing attributes like { "Size": "XL", "Color": "Red" }
    /// </summary>
    public string? Attributes { get; set; }
    
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }

    public virtual TblProduct Product { get; set; } = null!;
}
