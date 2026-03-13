using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Interfaces;

public interface IPromotionEngine
{
    Task<decimal> CalculateDiscountAsync(List<CartItemDto> items, string? couponCode = null);
    Task<List<string>> GetApplicablePromotionsAsync(List<CartItemDto> items);
}
