namespace VNVTStore.Application.DTOs.Import;

public class ProductImportDto
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? StockQuantity { get; set; }
    public string? CategoryCode { get; set; }
    public string? Sku { get; set; }
    public bool? IsActive { get; set; }
    public decimal? Weight { get; set; }
    public string? Color { get; set; }
    public string? Power { get; set; }
    public string? Voltage { get; set; }
    public string? Material { get; set; }
    public string? Size { get; set; }
    public string? SupplierCode { get; set; }
}
