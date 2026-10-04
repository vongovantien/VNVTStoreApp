using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblPermission : BaseEntity
{
    public TblPermission()
    {
        TblRolePermissions = new HashSet<TblRolePermission>();
    }

    public string Name { get; set; } = null!;
    public string Module { get; set; } = null!;
    public string? Description { get; set; }

    // TblPermission has no CreatedAt/UpdatedAt/ModifiedType columns in DB
    [NotMapped] public new DateTime? CreatedAt { get; set; }
    [NotMapped] public new DateTime? UpdatedAt { get; set; }
    [NotMapped] public new string? ModifiedType { get; set; }
    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    public virtual ICollection<TblRolePermission> TblRolePermissions { get; set; }

    public static TblPermission Create(string name, string module, string? description = null)
    {
        return new TblPermission
        {
            Code = CodeGenerator.New(),
            Name = name,
            Module = module,
            Description = description
        };
    }
}
