using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblMenu : IEntity
{
    public TblMenu()
    {
        TblRoleMenus = new HashSet<TblRoleMenu>();
    }

    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string GroupCode { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? CreatedAt { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? UpdatedAt { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? ModifiedType { get; set; }

    public virtual ICollection<TblRoleMenu> TblRoleMenus { get; set; }
}
    