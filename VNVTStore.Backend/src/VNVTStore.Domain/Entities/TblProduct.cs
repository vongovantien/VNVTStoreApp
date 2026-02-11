using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblProduct : IEntity
{
    private TblProduct() 
    {
        TblCartItems = new List<TblCartItem>();
        TblOrderItems = new List<TblOrderItem>();
        TblProductPromotions = new List<TblProductPromotion>();
        TblQuotes = new List<TblQuote>();
    }

    public string Code { get; set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }
    
    public decimal? WholesalePrice { get; private set; } // Added per UI req

    public decimal? CostPrice { get; private set; }

    public int? StockQuantity { get; private set; }

    public string? CategoryCode { get; private set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ModifiedType { get; set; }

    public string? SupplierCode { get; private set; }

    public string? BrandCode { get; private set; }
    public string? BaseUnit { get; private set; }
    public int? MinStockLevel { get; private set; }
    public string? BinLocation { get; private set; }
    public decimal? VatRate { get; private set; }
    public string? CountryOfOrigin { get; private set; }

    public virtual TblCategory? CategoryCodeNavigation { get; private set; }

    public virtual TblSupplier? SupplierCodeNavigation { get; private set; }
    public virtual TblBrand? Brand { get; private set; }

    public virtual ICollection<TblCartItem> TblCartItems { get; private set; }

    public virtual ICollection<TblOrderItem> TblOrderItems { get; private set; }

    public virtual ICollection<TblProductPromotion> TblProductPromotions { get; private set; }

    public virtual ICollection<TblQuote> TblQuotes { get; private set; }
    
    public virtual ICollection<TblProductDetail> TblProductDetails { get; private set; } = new List<TblProductDetail>();
    public virtual ICollection<TblProductUnit> TblProductUnits { get; private set; } = new List<TblProductUnit>();
    public virtual ICollection<TblProductTag> TblProductTags { get; private set; } = new List<TblProductTag>();
    public virtual ICollection<TblProductVariant> TblProductVariants { get; private set; } = new List<TblProductVariant>();

    public static TblProduct Create(string name, decimal price, decimal? wholesalePrice, int stock, string? categoryCode, decimal? costPrice, 
        string? supplierCode, string? brandCode = null, string? baseUnit = null)
    {
         return new TblProduct
         {
             Code = Guid.NewGuid().ToString("N").Substring(0, 10),
             Name = name,
             Price = price,
             WholesalePrice = wholesalePrice,
             StockQuantity = stock,
             CategoryCode = categoryCode,
             CostPrice = costPrice,
             SupplierCode = supplierCode,
             BrandCode = brandCode,
             BaseUnit = baseUnit,
             IsActive = true,
             CreatedAt = DateTime.UtcNow,
             UpdatedAt = DateTime.UtcNow // Initialize UpdatedAt
         };
    }

    public void UpdateInfo(string name, decimal price, decimal? wholesalePrice, string? description, string? categoryCode, decimal? costPrice, int? stockQuantity,
        string? supplierCode, string? brandCode, string? baseUnit, int? minStockLevel, string? binLocation, decimal? vatRate, string? countryOfOrigin)
    {
        Name = name;
        Price = price;
        WholesalePrice = wholesalePrice;
        Description = description;
        CategoryCode = categoryCode;
        CostPrice = costPrice;
        if (stockQuantity.HasValue) StockQuantity = stockQuantity.Value;
        
        // New fields
        SupplierCode = supplierCode;
        BrandCode = brandCode;
        BaseUnit = baseUnit;
        MinStockLevel = minStockLevel;
        BinLocation = binLocation;
        VatRate = vatRate;
        CountryOfOrigin = countryOfOrigin;
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeductStock(int quantity)
    {
        if (StockQuantity < quantity) 
             throw new InvalidOperationException($"Insufficient stock for product {Name}.");
        
        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RestoreStock(int quantity)
    {
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFromImport(string name, decimal price, decimal? wholesalePrice, int? stock, string? categoryCode, string? description, bool? isActive, string? supplierCode, string? brandCode)
    {
        Name = name;
        Price = price;
        WholesalePrice = wholesalePrice;
        StockQuantity = stock;
        CategoryCode = categoryCode;
        Description = description;
        if (isActive.HasValue) IsActive = isActive.Value;
        SupplierCode = supplierCode;
        BrandCode = brandCode;
        UpdatedAt = DateTime.UtcNow;
    }
}
