using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblSystemSecret : IEntity
{
    [Key]
    [MaxLength(100)]
    public string Code { get; set; } = null!; // e.g., "FIREBASE_KEY", "CLOUDINARY_API_KEY"

    public string? SecretValue { get; set; } // Can be JSON or string value

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool IsEncrypted { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }
}
