using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public partial class TblReview : BaseEntity
{
    public string? OrderItemCode { get; set; }
    public string UserCode { get; set; } = null!;
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool? IsApproved { get; set; }
    public string? ProductCode { get; set; }
    public string? ParentCode { get; set; }

    // No IsFixed/CreatedBy/UpdatedBy columns in DB yet
    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    public virtual TblOrderItem? OrderItemCodeNavigation { get; set; }
    public virtual TblProduct? ProductCodeNavigation { get; set; }
    public virtual TblUser UserCodeNavigation { get; set; } = null!;
    public virtual TblReview? ParentNavigation { get; set; }
    public virtual ICollection<TblReview> InverseParentNavigation { get; set; } = new List<TblReview>();
}
