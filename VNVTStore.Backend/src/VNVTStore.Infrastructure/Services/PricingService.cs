using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class PricingService : IPricingService
{
    private readonly IRepository<TblProduct> _productRepo;
    private readonly IRepository<TblProductUnit> _productUnitRepo;
    private readonly IRepository<TblUser> _userRepo;
    private readonly ICacheService _cacheService;

    public PricingService(IRepository<TblProduct> productRepo, IRepository<TblProductUnit> productUnitRepo, IRepository<TblUser> userRepo, ICacheService cacheService)
    {
        _productRepo = productRepo;
        _productUnitRepo = productUnitRepo;
        _userRepo = userRepo;
        _cacheService = cacheService;
    }

    public async Task<decimal> CalculatePriceAsync(string productCode, string? unitCode, string? userCode)
    {
        // 1. Get Product (Cached)
        var product = await _productRepo.AsQueryable().Include(p => p.TblProductUnits).FirstOrDefaultAsync(p => p.Code == productCode);
        if (product == null) throw new KeyNotFoundException($"Product {productCode} not found");

        decimal finalPrice = product.Price;

        // 2. Check User Role for Wholesale Price
        if (!string.IsNullOrEmpty(userCode))
        {
            var user = await _userRepo.GetByCodeAsync(userCode);
            // Example logic: Contractors get Wholesale Price
            // In a real app, "Contractor" could be a specific RoleCode or CustomerGroup
            if (user != null && (user.RoleCode == "CONTRACTOR" || user.Role == UserRole.Admin)) 
            {
                if (product.WholesalePrice.HasValue && product.WholesalePrice > 0)
                {
                    finalPrice = product.WholesalePrice.Value;
                }
            }
        }

        // 3. Unit Conversion
        // If a specific unit is requested (e.g. "Box" instead of "Piece")
        if (!string.IsNullOrEmpty(unitCode))
        {
            var unitConfig = product.TblProductUnits.FirstOrDefault(u => u.UnitCode == unitCode && u.IsActive);
            if (unitConfig != null)
            {
                // Option A: Use override price if set
                if (unitConfig.Price > 0)
                {
                    finalPrice = unitConfig.Price;
                }
                // Option B: Calculate from conversion rate (if price not explicitly set, or as fallback)
                else
                {
                    finalPrice *= unitConfig.ConversionRate;
                }
            }
        }

        return finalPrice;
    }

    public Task<decimal> GetDiscountForUserAsync(string userCode, decimal totalOrderAmount)
    {
        // Placeholder for Volume Discount Logic
        return Task.FromResult(0m); 
    }
}
