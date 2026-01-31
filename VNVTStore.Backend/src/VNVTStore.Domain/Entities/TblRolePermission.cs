namespace VNVTStore.Domain.Entities;

public class TblRolePermission
{
    public string RoleCode { get; set; } = null!;
    public string PermissionCode { get; set; } = null!;

    public virtual TblRole RoleCodeNavigation { get; set; } = null!;
    public virtual TblPermission PermissionCodeNavigation { get; set; } = null!;
}
