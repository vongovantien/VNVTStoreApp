using System;
using System.Collections.Generic;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblCart : IEntity
{
    private TblCart() 
    {
        TblCartItems = new List<TblCartItem>();
    }

    public string Code { get; set; } = null!;

    public string UserCode { get; private set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ModifiedType { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<TblCartItem> TblCartItems { get; private set; }

    public virtual TblUser UserCodeNavigation { get; private set; } = null!;

    public static TblCart Create(string userCode)
    {
        var now = DateTime.UtcNow;
        return new TblCart
        {
            Code = $"CRT{Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()}",
            UserCode = userCode,
            CreatedAt = now,
            UpdatedAt = now,
            ModifiedType = "ADD",
            IsActive = true,
            TblCartItems = new List<TblCartItem>()
        };
    }

    public void AddItem(string productCode, int quantity, string? size, string? color, int maxStock)
    {
        var existingItem = TblCartItems.FirstOrDefault(i => 
            i.ProductCode == productCode && 
            i.Size == size && 
            i.Color == color);

        if (existingItem != null)
        {
            if (existingItem.Quantity + quantity > maxStock)
            {
                throw new InvalidOperationException($"Cannot add more items. Max stock is {maxStock}.");
            }
            existingItem.AddQuantity(quantity);
        }
        else
        {
            if (quantity > maxStock)
            {
                 throw new InvalidOperationException($"Cannot add more items. Max stock is {maxStock}.");
            }
            var newItem = TblCartItem.Create(Code, productCode, quantity, size, color);
            TblCartItems.Add(newItem);
        }
    }

    public void UpdateItem(string cartItemCode, int quantity, int maxStock)
    {
        var item = TblCartItems.FirstOrDefault(i => i.Code == cartItemCode);
        if (item == null) throw new KeyNotFoundException("Cart item not found.");

        if (quantity <= 0)
        {
            TblCartItems.Remove(item);
        }
        else
        {
            if (quantity > maxStock)
            {
                 throw new InvalidOperationException($"Cannot update quantity. Max stock is {maxStock}.");
            }
            item.UpdateQuantity(quantity);
        }
    }

    public void RemoveItem(string cartItemCode)
    {
        var item = TblCartItems.FirstOrDefault(i => i.Code == cartItemCode);
        if (item != null)
        {
            TblCartItems.Remove(item);
        }
    }

    public void Clear()
    {
        TblCartItems.Clear();
    }
}
