using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Interfaces;

public interface IPricingService
{
    /// <summary>
    /// Calculate product price based on user role and unit
    /// </summary>
    Task<decimal> CalculatePriceAsync(string productCode, string? unitCode, string? userCode);

    /// <summary>
    /// Check if user reaches a discount tier
    /// </summary>
    Task<decimal> GetDiscountForUserAsync(string userCode, decimal totalOrderAmount);
}
