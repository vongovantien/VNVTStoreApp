namespace VNVTStore.Application.DTOs.Import;

public class ProductImportDto
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? WholesalePrice { get; set; }
    public int? StockQuantity { get; set; }
    public string? CategoryCode { get; set; }

    public bool? IsActive { get; set; }
    public string? BrandCode { get; set; }
    public string? BaseUnit { get; set; }
    public string? SupplierCode { get; set; }
}
