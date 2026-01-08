using System;
using System.Collections.Generic;

namespace VNVTStore.Infrastructure;

public partial class TblProduct
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal? CostPrice { get; set; }

    public int? StockQuantity { get; set; }

    public string? CategoryCode { get; set; }

    public string? Sku { get; set; }

    public decimal? Weight { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? SupplierCode { get; set; }

    public virtual TblCategory? CategoryCodeNavigation { get; set; }

    public virtual TblSupplier? SupplierCodeNavigation { get; set; }

    public virtual ICollection<TblCartItem> TblCartItems { get; set; } = new List<TblCartItem>();

    public virtual ICollection<TblOrderItem> TblOrderItems { get; set; } = new List<TblOrderItem>();

    public virtual ICollection<TblProductImage> TblProductImages { get; set; } = new List<TblProductImage>();

    public virtual ICollection<TblProductPromotion> TblProductPromotions { get; set; } = new List<TblProductPromotion>();
}
