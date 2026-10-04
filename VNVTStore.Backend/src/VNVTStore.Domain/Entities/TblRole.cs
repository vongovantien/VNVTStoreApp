using System.Collections.Generic;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblRole : BaseEntity
{
    public TblRole()
    {
        TblRolePermissions = new HashSet<TblRolePermission>();
        TblRoleMenus = new HashSet<TblRoleMenu>();
        TblUsers = new HashSet<TblUser>();
    }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public virtual ICollection<TblRolePermission> TblRolePermissions { get; set; }
    public virtual ICollection<TblRoleMenu> TblRoleMenus { get; set; }
    public virtual ICollection<TblUser> TblUsers { get; set; }

    public static TblRole Create(string name, string? description = null)
    {
        return new TblRole
        {
            Code = CodeGenerator.NewUpper(),
            Name = name,
            Description = description,
            IsActive = true
        };
    }
}
