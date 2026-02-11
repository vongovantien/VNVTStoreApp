namespace VNVTStore.Domain.Entities;

public class TblRoleMenu
{
    public string RoleCode { get; set; } = null!;
    public string MenuCode { get; set; } = null!;

    public virtual TblRole RoleCodeNavigation { get; set; } = null!;
    public virtual TblMenu MenuCodeNavigation { get; set; } = null!;
}
