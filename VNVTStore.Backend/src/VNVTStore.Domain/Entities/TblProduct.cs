using System;
using System.Collections.Generic;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblProduct : IEntity
{
    private TblProduct() 
    {
        TblCartItems = new List<TblCartItem>();
        TblOrderItems = new List<TblOrderItem>();
        TblProductImages = new List<TblProductImage>();
        TblProductPromotions = new List<TblProductPromotion>();
        TblQuotes = new List<TblQuote>();
    }

    public string Code { get; set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public decimal? CostPrice { get; private set; }

    public int? StockQuantity { get; private set; }

    public string? CategoryCode { get; private set; }

    public string? Sku { get; private set; }

    public decimal? Weight { get; private set; }

    public string? Color { get; private set; }

    public string? Power { get; private set; }

    public string? Voltage { get; private set; }

    public string? Material { get; private set; }

    public string? Size { get; private set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ModifiedType { get; set; }

    public string? SupplierCode { get; private set; }

    public virtual TblCategory? CategoryCodeNavigation { get; private set; }

    public virtual TblSupplier? SupplierCodeNavigation { get; private set; }

    public virtual ICollection<TblCartItem> TblCartItems { get; private set; }

    public virtual ICollection<TblOrderItem> TblOrderItems { get; private set; }

    public virtual ICollection<TblProductImage> TblProductImages { get; private set; }

    public virtual ICollection<TblProductPromotion> TblProductPromotions { get; private set; }

    public virtual ICollection<TblQuote> TblQuotes { get; private set; }

    public static TblProduct Create(string name, decimal price, int stock, string? categoryCode, string? sku, decimal? costPrice, 
        decimal? weight, string? supplierCode, string? color, string? power, string? voltage, string? material, string? size)
    {
         return new TblProduct
         {
             Code = Guid.NewGuid().ToString("N").Substring(0, 10),
             Name = name,
             Price = price,
             StockQuantity = stock,
             CategoryCode = categoryCode,
             Sku = sku,
             CostPrice = costPrice,
             Weight = weight, 
             SupplierCode = supplierCode,
             Color = color,
             Power = power,
             Voltage = voltage,
             Material = material,
             Size = size,
             IsActive = true,
             CreatedAt = DateTime.UtcNow,
             UpdatedAt = DateTime.UtcNow // Initialize UpdatedAt
         };
    }

    public void UpdateInfo(string name, decimal price, string? description, string? categoryCode, decimal? costPrice, int? stockQuantity,
        decimal? weight, string? supplierCode, string? color, string? power, string? voltage, string? material, string? size, string? sku)
    {
        Name = name;
        Price = price;
        Description = description;
        CategoryCode = categoryCode;
        CostPrice = costPrice;
        if (stockQuantity.HasValue) StockQuantity = stockQuantity.Value;
        
        // New fields
        Weight = weight;
        SupplierCode = supplierCode;
        Color = color;
        Power = power;
        Voltage = voltage;
        Material = material;
        Size = size;
        Sku = sku;
        
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

    public void UpdateFromImport(string name, decimal price, int? stock, string? categoryCode, string? sku, string? description, bool? isActive, decimal? weight, string? color, string? power, string? voltage, string? material, string? size, string? supplierCode)
    {
        Name = name;
        Price = price;
        StockQuantity = stock;
        CategoryCode = categoryCode;
        Sku = sku;
        Description = description;
        if (isActive.HasValue) IsActive = isActive.Value;
        Weight = weight;
        Color = color;
        Power = power;
        Voltage = voltage;
        Material = material;
        Size = size;
        SupplierCode = supplierCode;
        UpdatedAt = DateTime.UtcNow;
    }
}
