using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblMenu : BaseEntity
{
    public TblMenu()
    {
        TblRoleMenus = new HashSet<TblRoleMenu>();
    }

    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string GroupCode { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }

    // TblMenu has no audit columns in DB
    [NotMapped] public new DateTime? CreatedAt { get; set; }
    [NotMapped] public new DateTime? UpdatedAt { get; set; }
    [NotMapped] public new string? ModifiedType { get; set; }
    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    public virtual ICollection<TblRoleMenu> TblRoleMenus { get; set; }
}
