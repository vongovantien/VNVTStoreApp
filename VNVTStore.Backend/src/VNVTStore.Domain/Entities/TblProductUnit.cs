using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblProductUnit : BaseEntity
{
    public string ProductCode { get; set; } = null!;
    public string UnitCode { get; set; } = null!;
    public decimal ConversionRate { get; set; }
    public decimal Price { get; set; }
    public bool IsBaseUnit { get; set; }

    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    public virtual TblProduct Product { get; set; } = null!;
    public virtual TblUnit Unit { get; set; } = null!;
}
