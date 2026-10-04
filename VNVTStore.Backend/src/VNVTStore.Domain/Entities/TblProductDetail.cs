using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Domain.Entities;

public class TblProductDetail : BaseEntity
{
    public string ProductCode { get; set; } = null!;
    public ProductDetailType DetailType { get; set; } = ProductDetailType.SPEC;
    public string SpecName { get; set; } = null!;
    public string SpecValue { get; set; } = null!;

    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    public virtual TblProduct Product { get; set; } = null!;
}
