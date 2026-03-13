using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblPermission : IEntity
{
    public TblPermission()
    {
        TblRolePermissions = new HashSet<TblRolePermission>();
    }

    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Module { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? CreatedAt { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? UpdatedAt { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? ModifiedType { get; set; }

    public virtual ICollection<TblRolePermission> TblRolePermissions { get; set; }

    public static TblPermission Create(string name, string module, string? description = null)
    {
        return new TblPermission
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            Name = name,
            Module = module,
            Description = description
        };
    }
}
