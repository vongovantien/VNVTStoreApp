using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly IRepository<TblCoupon> _couponRepository;
    private readonly IMapper _mapper;

    public CouponService(IRepository<TblCoupon> couponRepository, IMapper mapper)
    {
        _couponRepository = couponRepository;
        _mapper = mapper;
    }

    public async Task<Result<CouponDto>> ValidateCouponAsync(string couponCode, decimal orderAmount, CancellationToken cancellationToken = default)
    {
        var coupon = await _couponRepository.AsQueryable()
            .Include(c => c.PromotionCodeNavigation)
            .FirstOrDefaultAsync(c => c.Code == couponCode, cancellationToken);

        if (coupon == null)
            return Result.Failure<CouponDto>(Error.NotFound(MessageConstants.Coupon, couponCode));

        var promotion = coupon.PromotionCodeNavigation;

        // Validate promotion exists and is active
        if (promotion == null || promotion.IsActive != true)
            return Result.Failure<CouponDto>(Error.Validation(MessageConstants.CouponNotActive));

        // Validate date range
        var now = DateTime.Now;
        if (now < promotion.StartDate || now > promotion.EndDate)
            return Result.Failure<CouponDto>(Error.Validation(MessageConstants.CouponExpired));

        // Validate minimum order amount
        if (promotion.MinOrderAmount.HasValue && orderAmount < promotion.MinOrderAmount)
            return Result.Failure<CouponDto>(Error.Validation(MessageConstants.ValidationFailed, 
                $"Minimum order amount is {promotion.MinOrderAmount}"));

        // Validate usage limit
        if (promotion.UsageLimit.HasValue && coupon.UsageCount >= promotion.UsageLimit)
            return Result.Failure<CouponDto>(Error.Validation(MessageConstants.CouponLimitReached));

        var dto = _mapper.Map<CouponDto>(coupon);
        dto.IsValid = true;
        return Result.Success(dto);
    }
}
