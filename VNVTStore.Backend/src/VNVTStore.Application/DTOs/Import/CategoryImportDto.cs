namespace VNVTStore.Application.DTOs.Import;

public class CategoryImportDto
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public string? ParentCategoryCode { get; set; }
}
