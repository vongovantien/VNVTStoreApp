using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblProductVariant : BaseEntity
{
    public string ProductCode { get; set; } = null!;
    public string SKU { get; set; } = null!;

    /// <summary>JSON string storing attributes like { "Size": "XL", "Color": "Red" }</summary>
    public string? Attributes { get; set; }

    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    // No IsFixed/CreatedBy/UpdatedBy columns for this table in DB yet
    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    public virtual TblProduct Product { get; set; } = null!;
}
