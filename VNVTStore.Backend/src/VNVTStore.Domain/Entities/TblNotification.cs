using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

[Table("TblNotification")]
public class TblNotification : BaseEntity
{
    [StringLength(100)]
    public string UserCode { get; set; } = null!;

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    [StringLength(50)]
    public string Type { get; set; } = "SYSTEM";

    public bool IsRead { get; set; } = false;

    [StringLength(255)]
    public string? Link { get; set; }

    // No IsFixed/CreatedBy/UpdatedBy columns for this table in DB
    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    [ForeignKey("UserCode")]
    public virtual TblUser? UserCodeNavigation { get; set; }
}
