using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblCartItem
{
    private TblCartItem() { } // For EF Core

    public string Code { get; private set; } = null!;

    public string CartCode { get; private set; } = null!;

    public string ProductCode { get; private set; } = null!;

    public int Quantity { get; private set; }

    public string? Size { get; private set; }

    public string? Color { get; private set; }

    public DateTime? AddedAt { get; private set; }

    public virtual TblCart CartCodeNavigation { get; private set; } = null!;

    public virtual TblProduct ProductCodeNavigation { get; private set; } = null!;

    public static TblCartItem Create(string cartCode, string productCode, int quantity, string? size, string? color)
    {
        return new TblCartItem
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            CartCode = cartCode,
            ProductCode = productCode,
            Quantity = quantity,
            Size = size,
            Color = color,
            AddedAt = DateTime.UtcNow
        };
    }

    public void AddQuantity(int quantity)
    {
        Quantity += quantity;
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
    }
}
