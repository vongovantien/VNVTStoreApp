namespace VNVTStore.Application.DTOs;

public class UnitDto : IBaseDto
{
    public string Code { get; set; } = null!; // ProductUnit Code
    public string ProductCode { get; set; } = null!;
    public string? UnitCode { get; set; } // MasterUnit Code
    public string UnitName { get; set; } = null!; // MasterUnit Name
    public decimal ConversionRate { get; set; }
    public decimal Price { get; set; }
    public bool IsBaseUnit { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUnitDto
{
    public string? ProductCode { get; set; } 
    public string UnitName { get; set; } = null!;
    public decimal ConversionRate { get; set; }
    public decimal Price { get; set; }
    public bool IsBaseUnit { get; set; }
}

public class UpdateUnitDto
{
    public string? UnitName { get; set; }
    public decimal? ConversionRate { get; set; }
    public decimal? Price { get; set; }
    public bool? IsActive { get; set; }
}
