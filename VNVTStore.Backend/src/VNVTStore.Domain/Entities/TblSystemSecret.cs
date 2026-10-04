using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblSystemSecret : BaseEntity
{
    public string? SecretValue { get; set; } // Can be JSON or string value

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool IsEncrypted { get; set; } = false;
}
