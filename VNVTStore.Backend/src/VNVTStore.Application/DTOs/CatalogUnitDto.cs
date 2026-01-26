namespace VNVTStore.Application.DTOs;

// ==================== CATALOG UNIT DTO ====================
public class CatalogUnitDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateCatalogUnitDto
{
    public string Name { get; set; } = null!;
}

public class UpdateCatalogUnitDto
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
