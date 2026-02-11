using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblRole : IEntity
{
    public TblRole()
    {
        TblRolePermissions = new HashSet<TblRolePermission>();
        TblRoleMenus = new HashSet<TblRoleMenu>();
        TblUsers = new HashSet<TblUser>();
    }

    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedType { get; set; }

    public virtual ICollection<TblRolePermission> TblRolePermissions { get; set; }
    public virtual ICollection<TblRoleMenu> TblRoleMenus { get; set; }
    public virtual ICollection<TblUser> TblUsers { get; set; }

    public static TblRole Create(string name, string? description = null)
    {
        return new TblRole
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            Name = name,
            Description = description,
            IsActive = true
        };
    }
}
