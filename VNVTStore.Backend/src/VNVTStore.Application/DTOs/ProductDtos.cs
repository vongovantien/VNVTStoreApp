namespace VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Attributes;
using VNVTStore.Domain.Enums;

public class ProductDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? CategoryCode { get; set; }
    [Reference("TblCategory", "CategoryCode", "Name")]
    public string? CategoryName { get; set; }
    public int? StockQuantity { get; set; }

    public string? BrandCode { get; set; }
    [Reference("TblBrand", "BrandCode", "Name")]
    public string? Brand { get; set; }
    public string? BaseUnit { get; set; }
    public int? MinStockLevel { get; set; }
    public string? BinLocation { get; set; }
    public decimal? VatRate { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? SupplierCode { get; set; }
    [Reference("TblSupplier", "SupplierCode", "Name")]
    public string? SupplierName { get; set; }

    public bool? IsActive { get; set; }
    public bool IsNew { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? CreatedAt { get; set; }

    [ReferenceCollection(typeof(ProductDetailDto), "TblProductDetail", "ProductCode", "Code")]
    public List<ProductDetailDto>? Details { get; set; }

    [ReferenceCollection(typeof(ProductImageDto), "TblFile", "MasterCode", "Code", "MasterType", "Product")]
    public List<ProductImageDto>? ProductImages { get; set; }

    [ReferenceCollection(typeof(UnitDto), "TblProductUnit", "ProductCode", "Code")]
    public List<UnitDto>? ProductUnits { get; set; }

    [ReferenceCollection(typeof(ProductVariantDto), "TblProductVariant", "ProductCode", "Code")]
    public List<ProductVariantDto>? Variants { get; set; }

    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class ProductDetailDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public ProductDetailType DetailType { get; set; } = ProductDetailType.SPEC;
    public string SpecName { get; set; } = null!;
    public string SpecValue { get; set; } = null!;
}

public class CreateProductDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? CategoryCode { get; set; }
    public int? StockQuantity { get; set; }
    public string? BrandCode { get; set; }
    public string? BaseUnit { get; set; }
    public int? MinStockLevel { get; set; }
    public string? BinLocation { get; set; }
    public decimal? VatRate { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? SupplierCode { get; set; }
    public bool IsNew { get; set; }
    public bool IsFeatured { get; set; }
    public List<string>? Images { get; set; }
    public List<CreateProductDetailDto>? Details { get; set; }
    public List<CreateUnitDto>? ProductUnits { get; set; }
    public List<CreateProductVariantDto>? Variants { get; set; }
}

public class CreateProductDetailDto
{
    public ProductDetailType DetailType { get; set; } = ProductDetailType.SPEC;
    public string SpecName { get; set; } = null!;
    public string SpecValue { get; set; } = null!;
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? CategoryCode { get; set; }
    public int? StockQuantity { get; set; }
    public string? BrandCode { get; set; }
    public string? BaseUnit { get; set; }
    public int? MinStockLevel { get; set; }
    public string? BinLocation { get; set; }
    public decimal? VatRate { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? SupplierCode { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsNew { get; set; }
    public bool? IsFeatured { get; set; }
    public List<string>? Images { get; set; }
    public List<CreateProductDetailDto>? Details { get; set; } // reuse Create dto for list replacement
    public List<CreateUnitDto>? ProductUnits { get; set; }
    public List<CreateProductVariantDto>? Variants { get; set; }
}

public class ProductImageDto
{
    public string Code { get; set; } = null!;
    public string? MasterCode { get; set; }
    public string? ImageURL { get; set; }
    public string? AltText { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsPrimary { get; set; }
}

public class ProductStatsDto
{
    public int Total { get; set; }
    public int LowStock { get; set; }
    public int OutOfStock { get; set; }
}

public class ProductVariantDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string ProductCode { get; set; } = null!;
    public string SKU { get; set; } = null!;
    public string? Attributes { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProductVariantDto
{
    public string SKU { get; set; } = null!;
    public string? Attributes { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class UpdateProductVariantDto
{
    public string? SKU { get; set; }
    public string? Attributes { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
    public bool? IsActive { get; set; }
}
