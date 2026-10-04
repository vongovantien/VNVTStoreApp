using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

[Table("TblAuditLog")]
public class TblAuditLog : BaseEntity
{
    [StringLength(100)]
    public string? UserCode { get; set; }

    [StringLength(100)]
    public string Action { get; set; } = null!;

    [StringLength(255)]
    public string? Target { get; set; }

    public string? Detail { get; set; }

    [StringLength(50)]
    public string? IpAddress { get; set; }

    // No IsFixed/CreatedBy/UpdatedBy columns for this table in DB
    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    [ForeignKey("UserCode")]
    public virtual TblUser? UserCodeNavigation { get; set; }
}
