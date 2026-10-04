using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VNVTStore.Application.Services;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponValidationService _couponValidation;
    private readonly IRepository<TblCoupon> _couponRepo;
    private readonly IRepository<TblPromotion> _promotionRepo;

    public CouponsController(
        ICouponValidationService couponValidation,
        IRepository<TblCoupon> couponRepo,
        IRepository<TblPromotion> promotionRepo)
    {
        _couponValidation = couponValidation;
        _couponRepo = couponRepo;
        _promotionRepo = promotionRepo;
    }

    /// <summary>
    /// Validate coupon code — SINGLE SOURCE OF TRUTH
    /// Frontend gọi endpoint này trước khi apply coupon
    /// <summary>
    /// Validate coupon code — SINGLE SOURCE OF TRUTH
    /// Frontend gọi endpoint này trước khi apply coupon
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
        var effectiveCode = request.CouponCode?.Trim() ?? string.Empty;
        var effectiveTotal = request.OrderTotal > 0 ? request.OrderTotal : (request.OrderAmount ?? 0);

        // 1. Try finding coupon by code
        var coupon = await _couponRepo.GetByCodeAsync(effectiveCode);
        TblPromotion? promotion = null;

        if (coupon != null)
        {
            promotion = await _promotionRepo.GetByCodeAsync(coupon.PromotionCode ?? string.Empty);
        }
        else
        {
            // 2. Allow validating promotion code directly if no specific coupon code
            promotion = await _promotionRepo.GetByCodeAsync(effectiveCode);
            if (promotion != null)
            {
                coupon = new TblCoupon
                {
                    Code = promotion.Code,
                    PromotionCode = promotion.Code,
                    UsageCount = 0,
                    IsActive = promotion.IsActive
                };
            }
        }
        
        if (coupon == null || promotion == null)
        {
            return Ok(new
            {
                success = false,
                message = "Mã giảm giá không tồn tại",
                data = new { isValid = false, errors = new[] { "Mã giảm giá không tồn tại" } }
            });
        }

        // Get userId nếu authenticated
        string? userId = User.Identity?.IsAuthenticated == true 
            ? User.FindFirst("userId")?.Value 
            : null;

        // Validate
        var result = await _couponValidation.ValidateAsync(coupon, promotion, effectiveTotal, userId);

        return Ok(new
        {
            success = result.IsValid,
            message = result.IsValid ? "Áp dụng mã giảm giá thành công" : (result.Errors.FirstOrDefault() ?? "Mã giảm giá không hợp lệ"),
            data = new
            {
                isValid = result.IsValid,
                errors = result.Errors,
                discountAmount = result.DiscountAmount,
                finalAmount = result.FinalAmount,
                couponDetails = result.IsValid ? new
                {
                    code = coupon.Code,
                    promotionName = promotion.Name,
                    description = promotion.Description,
                    discountType = promotion.DiscountType,
                    discountValue = promotion.DiscountValue,
                    maxDiscountAmount = promotion.MaxDiscountAmount,
                    minOrderAmount = promotion.MinOrderAmount
                } : null
            }
        });
    }
}

public class ValidateCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public decimal? OrderAmount { get; set; }
}
