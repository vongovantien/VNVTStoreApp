using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

[Table("TblAuditLog")]
public class TblAuditLog : IEntity
{
    [Key]
    [Column("Code")]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Column("UserCode")]
    [StringLength(100)]
    public string? UserCode { get; set; }

    [Column("Action")]
    [StringLength(100)]
    public string Action { get; set; } = null!; // e.g., "LOGIN", "UPDATE_PRODUCT", "DELETE_ORDER"

    [Column("Target")]
    [StringLength(255)]
    public string? Target { get; set; } // e.g., "Product:PRD0001"

    [Column("Detail")]
    public string? Detail { get; set; } // JSON or description of change

    [Column("IpAddress")]
    [StringLength(50)]
    public string? IpAddress { get; set; }

    [Column("CreatedAt")]
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("ModifiedType")]
    [StringLength(10)]
    public string? ModifiedType { get; set; } = "ADD";

    [ForeignKey("UserCode")]
    public virtual TblUser? UserCodeNavigation { get; set; }
}
