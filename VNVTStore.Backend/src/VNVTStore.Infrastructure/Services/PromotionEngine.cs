using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Infrastructure.Persistence;

namespace VNVTStore.Infrastructure.Services;

public class PromotionEngine : IPromotionEngine
{
    private readonly ApplicationDbContext _context;

    public PromotionEngine(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> CalculateDiscountAsync(List<CartItemDto> items, string? couponCode = null)
    {
        decimal totalDiscount = 0;

        // 1. Internal Logic: Buy 2 Get 1 Free (Lowest price item in the group is free)
        // Group by category to apply "Buy 2 Get 1" per category
        var productCodes = items.Select(i => i.ProductCode).ToList();
        var products = await _context.TblProducts
            .Where(p => productCodes.Contains(p.Code))
            .ToListAsync();

        var itemDetails = items.Join(products, 
            i => i.ProductCode, 
            p => p.Code, 
            (i, p) => new { i.Quantity, p.Price, p.CategoryCode })
            .ToList();

        var categoryGroups = itemDetails.GroupBy(x => x.CategoryCode);

        foreach (var group in categoryGroups)
        {
            var flatItems = group.SelectMany(x => Enumerable.Repeat(x.Price, x.Quantity)).OrderByDescending(p => p).ToList();
            
            // For every 3 items, the cheapest one is free
            int freeItemsCount = flatItems.Count / 3;
            if (freeItemsCount > 0)
            {
                totalDiscount += flatItems.TakeLast(freeItemsCount).Sum();
            }
        }

        return totalDiscount;
    }

    public Task<List<string>> GetApplicablePromotionsAsync(List<CartItemDto> items)
    {
        var promos = new List<string>();
        // Simple mock for now
        promos.Add("Ưu đãi mua 2 tặng 1 đang được áp dụng!");
        return Task.FromResult(promos);
    }
}
