namespace VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Attributes;

public class CategoryDto : IBaseDto
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageURL { get; set; }
    public string? ParentCode { get; set; }
    [Reference("TblCategory", "ParentCode", "Name")]
    public string? ParentName { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateCategoryDto
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageURL { get; set; }
    public string? ParentCode { get; set; }
}

public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageURL { get; set; }
    public string? ParentCode { get; set; }
    public bool? IsActive { get; set; }
}

public class CategoryStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Main { get; set; }
}
