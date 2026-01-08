using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Coupons.Commands;
using VNVTStore.Application.Coupons.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Coupons.Handlers;

public class CouponHandlers :
    IRequestHandler<ValidateCouponCommand, Result<CouponDto>>,
    IRequestHandler<GetCouponByCodeQuery, Result<CouponDto>>
{
    private readonly IRepository<TblCoupon> _couponRepository;
    private readonly IMapper _mapper;

    public CouponHandlers(
        IRepository<TblCoupon> couponRepository,
        IMapper mapper)
    {
        _couponRepository = couponRepository;
        _mapper = mapper;
    }

    public async Task<Result<CouponDto>> Handle(ValidateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.AsQueryable()
            .Include(c => c.PromotionCodeNavigation)
            .FirstOrDefaultAsync(c => c.Code == request.CouponCode, cancellationToken);

        if (coupon == null)
            return Result.Failure<CouponDto>(Error.NotFound("Coupon", request.CouponCode));

        var promotion = coupon.PromotionCodeNavigation;

        // Validate promotion exists and is active
        if (promotion == null || promotion.IsActive != true)
            return Result.Failure<CouponDto>(Error.Validation("Coupon", "Coupon is not active"));

        // Validate date range
        var now = DateTime.UtcNow;
        if (now < promotion.StartDate || now > promotion.EndDate)
            return Result.Failure<CouponDto>(Error.Validation("Coupon", "Coupon has expired or not yet started"));

        // Validate minimum order amount
        if (promotion.MinOrderAmount.HasValue && request.OrderAmount < promotion.MinOrderAmount)
            return Result.Failure<CouponDto>(Error.Validation("Coupon", 
                $"Minimum order amount is {promotion.MinOrderAmount}"));

        // Validate usage limit
        if (promotion.UsageLimit.HasValue && coupon.UsageCount >= promotion.UsageLimit)
            return Result.Failure<CouponDto>(Error.Validation("Coupon", "Coupon usage limit reached"));

        var dto = _mapper.Map<CouponDto>(coupon);
        dto.IsValid = true;
        return Result.Success(dto);
    }

    public async Task<Result<CouponDto>> Handle(GetCouponByCodeQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.AsQueryable()
            .Include(c => c.PromotionCodeNavigation)
            .FirstOrDefaultAsync(c => c.Code == request.CouponCode, cancellationToken);

        if (coupon == null)
            return Result.Failure<CouponDto>(Error.NotFound("Coupon", request.CouponCode));

        return Result.Success(_mapper.Map<CouponDto>(coupon));
    }
}
