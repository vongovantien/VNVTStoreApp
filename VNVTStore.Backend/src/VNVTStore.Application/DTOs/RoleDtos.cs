namespace VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Attributes;

public class RoleDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    
    [ReferenceCollection(typeof(PermissionDto), "TblRolePermission", "RoleCode", "PermissionCode")]
    public List<PermissionDto> Permissions { get; set; } = new();
    
    [ReferenceCollection(typeof(MenuDto), "TblRoleMenu", "RoleCode", "MenuCode")]
    public List<MenuDto> Menus { get; set; } = new();
}

public class CreateRoleDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<string> PermissionCodes { get; set; } = new();
    public List<string> MenuCodes { get; set; } = new();
}

public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? PermissionCodes { get; set; }
    public List<string>? MenuCodes { get; set; }
}

public class PermissionDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Module { get; set; } = null!;
    public string? Description { get; set; }
}

public class RolePermissionDto
{
    public string RoleCode { get; set; } = null!;
    public string PermissionCode { get; set; } = null!;
}

public class MenuDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string GroupCode { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
