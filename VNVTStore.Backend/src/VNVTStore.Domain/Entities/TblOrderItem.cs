using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblOrderItem
{
    private TblOrderItem() { }

    public string Code { get; private set; } = null!;

    public string OrderCode { get; private set; } = null!;

    public string? ProductCode { get; private set; }

    public int Quantity { get; private set; }

    public string? Size { get; private set; }

    public string? Color { get; private set; }
    
    public string? ProductName { get; private set; }
    public string? ProductImage { get; private set; }

    public decimal PriceAtOrder { get; private set; }

    public decimal? DiscountAmount { get; private set; }

    public virtual TblOrder OrderCodeNavigation { get; private set; } = null!;

    public virtual TblProduct? ProductCodeNavigation { get; private set; }

    public virtual ICollection<TblReview> TblReviews { get; private set; } = new List<TblReview>();

    public static TblOrderItem Create(string productCode, string productName, string? productImage, int quantity, decimal priceAtOrder, string? size, string? color)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (priceAtOrder < 0) throw new ArgumentException("Price cannot be negative.", nameof(priceAtOrder));

        return new TblOrderItem
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            ProductCode = productCode,
            ProductName = productName,
            ProductImage = productImage,
            Quantity = quantity,
            PriceAtOrder = priceAtOrder,
            Size = size,
            Color = color,
            DiscountAmount = 0
        };
    }
}
