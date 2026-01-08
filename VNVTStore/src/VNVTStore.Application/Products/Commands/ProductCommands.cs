using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Products.Commands;

/// <summary>
/// Command tạo Product mới
/// </summary>
public record CreateProductCommand(CreateProductDto Dto) 
    : CreateCommand<CreateProductDto, ProductDto>(Dto);

/// <summary>
/// Command cập nhật Product
/// </summary>
public record UpdateProductCommand(string Code, UpdateProductDto Dto) 
    : UpdateCommand<UpdateProductDto, ProductDto>(Code, Dto);

/// <summary>
/// Command xóa Product (soft delete)
/// </summary>
public record DeleteProductCommand(string Code) : DeleteCommand(Code);

// DTOs cho PostObject
public class CreateProductDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? CategoryCode { get; set; }
    public string? Sku { get; set; }
    public decimal? Weight { get; set; }
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? CostPrice { get; set; }
    public int? StockQuantity { get; set; }
    public string? CategoryCode { get; set; }
    public string? Sku { get; set; }
    public decimal? Weight { get; set; }
    public bool? IsActive { get; set; }
}
