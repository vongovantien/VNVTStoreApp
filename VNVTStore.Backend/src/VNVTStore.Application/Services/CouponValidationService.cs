using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Services;

/// <summary>
/// Centralized coupon validation logic — SINGLE SOURCE OF TRUTH
/// Frontend chỉ hiển thị, mọi validation chính thức đều qua service này
/// </summary>
public interface ICouponValidationService
{
    Task<CouponValidationResult> ValidateAsync(TblCoupon coupon, TblPromotion promotion, decimal orderTotal, string? userId = null);
}

public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}

public class CouponValidationService : ICouponValidationService
{
    public async Task<CouponValidationResult> ValidateAsync(
        TblCoupon coupon,
        TblPromotion promotion,
        decimal orderTotal, 
        string? userId = null)
    {
        var result = new CouponValidationResult();

        // 1. Check if coupon exists
        if (coupon == null || promotion == null)
        {
            result.Errors.Add("Mã giảm giá không tồn tại");
            return result;
        }

        // 2. Check if promotion is active
        if (!promotion.IsActive)
        {
            result.Errors.Add("Mã giảm giá đã bị vô hiệu hóa");
            return result;
        }

        // 3. Check date range
        var now = DateTime.UtcNow;
        if (now < promotion.StartDate)
        {
            result.Errors.Add($"Mã giảm giá chưa có hiệu lực (bắt đầu từ {promotion.StartDate:dd/MM/yyyy})");
            return result;
        }

        if (now > promotion.EndDate)
        {
            result.Errors.Add($"Mã giảm giá đã hết hạn ({promotion.EndDate:dd/MM/yyyy})");
            return result;
        }

        // 4. Check usage limit
        if (promotion.UsageLimit.HasValue && coupon.UsageCount >= promotion.UsageLimit.Value)
        {
            result.Errors.Add("Mã giảm giá đã hết lượt sử dụng");
            return result;
        }

        // 5. Check minimum order value
        if (promotion.MinOrderAmount.HasValue && orderTotal < promotion.MinOrderAmount.Value)
        {
            result.Errors.Add($"Đơn hàng tối thiểu {promotion.MinOrderAmount.Value:N0}₫ để áp dụng mã này");
            return result;
        }

        // 6. Calculate discount
        decimal discountAmount = 0;
        if (string.Equals(promotion.DiscountType, "percentage", StringComparison.OrdinalIgnoreCase))
        {
            discountAmount = orderTotal * (promotion.DiscountValue / 100m);
            
            // Apply max discount cap
            if (promotion.MaxDiscountAmount.HasValue && discountAmount > promotion.MaxDiscountAmount.Value)
            {
                discountAmount = promotion.MaxDiscountAmount.Value;
            }
        }
        else // fixed
        {
            discountAmount = promotion.DiscountValue;
        }

        // Discount cannot exceed order total
        if (discountAmount > orderTotal)
        {
            discountAmount = orderTotal;
        }

        result.IsValid = true;
        result.DiscountAmount = discountAmount;
        result.FinalAmount = orderTotal - discountAmount;

        return await Task.FromResult(result);
    }
}
