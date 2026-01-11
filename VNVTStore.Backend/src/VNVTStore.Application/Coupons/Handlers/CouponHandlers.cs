using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Coupons.Commands;
using VNVTStore.Application.Coupons.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Coupons.Handlers;

public class CouponHandlers : BaseHandler<TblCoupon>,
    IRequestHandler<ValidateCouponCommand, Result<CouponDto>>,
    IRequestHandler<GetByCodeQuery<CouponDto>, Result<CouponDto>>,
    IRequestHandler<CreateCommand<CreateCouponDto, CouponDto>, Result<CouponDto>>,
    IRequestHandler<DeleteCommand<TblCoupon>, Result>,
    IRequestHandler<GetPagedQuery<CouponDto>, Result<PagedResult<CouponDto>>>
{
    private readonly IRepository<TblPromotion> _promotionRepository;
    private readonly ICouponService _couponService;

    public CouponHandlers(
        IRepository<TblCoupon> couponRepository,
        IRepository<TblPromotion> promotionRepository,
        ICouponService couponService,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(couponRepository, unitOfWork, mapper)
    {
        _promotionRepository = promotionRepository;
        _couponService = couponService;
    }

    public async Task<Result<CouponDto>> Handle(ValidateCouponCommand request, CancellationToken cancellationToken)
    {
        return await _couponService.ValidateCouponAsync(request.CouponCode, request.OrderAmount, cancellationToken);
    }

    public async Task<Result<CouponDto>> Handle(GetByCodeQuery<CouponDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<CouponDto>(
            request.Code, 
            MessageConstants.Coupon, 
            cancellationToken,
            includes: q => q.Include(c => c.PromotionCodeNavigation));
    }

    public async Task<Result<CouponDto>> Handle(CreateCommand<CreateCouponDto, CouponDto> request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.Dto.PromotionCode))
        {
            var promotion = await _promotionRepository.GetByCodeAsync(request.Dto.PromotionCode, cancellationToken);
            if (promotion == null)
            {
                return Result.Failure<CouponDto>(Error.NotFound(MessageConstants.Coupon, request.Dto.PromotionCode));
            }
        }

        return await CreateAsync<CreateCouponDto, CouponDto>(
            request.Dto,
            cancellationToken,
            c => {
                c.Code = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
                c.UsageCount = 0;
            });
    }

    public async Task<Result> Handle(DeleteCommand<TblCoupon> request, CancellationToken cancellationToken)
    {
        return await DeleteAsync(request.Code, MessageConstants.Coupon, cancellationToken, softDelete: false);
    }

    public async Task<Result<PagedResult<CouponDto>>> Handle(GetPagedQuery<CouponDto> request, CancellationToken cancellationToken)
    {
        return await GetPagedAsync<CouponDto>(
            request.PageIndex,
            request.PageSize,
            cancellationToken,
            includes: q => q.Include(c => c.PromotionCodeNavigation),
            orderBy: q => q.OrderByDescending(c => c.Code));
    }
}
