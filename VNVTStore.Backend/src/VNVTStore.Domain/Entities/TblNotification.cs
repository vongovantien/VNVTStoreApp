using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

[Table("TblNotification")]
public class TblNotification : IEntity
{
    [Key]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [StringLength(100)]
    public string UserCode { get; set; } = null!;

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    [StringLength(50)]
    public string Type { get; set; } = "SYSTEM"; // INFO, SUCCESS, WARNING, ERROR, PROMO

    public bool IsRead { get; set; } = false;

    [StringLength(255)]
    public string? Link { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [StringLength(10)]
    public string? ModifiedType { get; set; } = "ADD";

    [ForeignKey("UserCode")]
    public virtual TblUser? UserCodeNavigation { get; set; }
}
