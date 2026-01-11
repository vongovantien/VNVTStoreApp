using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblProduct
{
    private TblProduct() 
    {
        TblCartItems = new List<TblCartItem>();
        TblOrderItems = new List<TblOrderItem>();
        TblProductImages = new List<TblProductImage>();
        TblProductPromotions = new List<TblProductPromotion>();
        TblQuotes = new List<TblQuote>();
    }

    public string Code { get; private set; } = null!;

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

    public bool? IsActive { get; private set; }

    public DateTime? CreatedAt { get; private set; }

    public string? SupplierCode { get; private set; }

    public virtual TblCategory? CategoryCodeNavigation { get; private set; }

    public virtual TblSupplier? SupplierCodeNavigation { get; private set; }

    public virtual ICollection<TblCartItem> TblCartItems { get; private set; }

    public virtual ICollection<TblOrderItem> TblOrderItems { get; private set; }

    public virtual ICollection<TblProductImage> TblProductImages { get; private set; }

    public virtual ICollection<TblProductPromotion> TblProductPromotions { get; private set; }

    public virtual ICollection<TblQuote> TblQuotes { get; private set; }

    public static TblProduct Create(string name, decimal price, int stock, string? categoryCode, string? sku)
    {
         return new TblProduct
         {
             Code = Guid.NewGuid().ToString("N").Substring(0, 10),
             Name = name,
             Price = price,
             StockQuantity = stock,
             CategoryCode = categoryCode,
             Sku = sku,
             IsActive = true,
             CreatedAt = DateTime.UtcNow
         };
    }

    public void UpdateInfo(string name, decimal price, string? description, string? categoryCode)
    {
        Name = name;
        Price = price;
        Description = description;
        CategoryCode = categoryCode;
    }

    public void DeductStock(int quantity)
    {
        if (StockQuantity < quantity) 
             throw new InvalidOperationException($"Insufficient stock for product {Name}.");
        
        StockQuantity -= quantity;
    }

    public void RestoreStock(int quantity)
    {
        StockQuantity += quantity;
    }
}
