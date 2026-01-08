using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Promotions.Commands;
using VNVTStore.Application.Promotions.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Promotions.Handlers;

public class PromotionHandlers :
    IRequestHandler<CreatePromotionCommand, Result<PromotionDto>>,
    IRequestHandler<UpdatePromotionCommand, Result<PromotionDto>>,
    IRequestHandler<DeletePromotionCommand, Result<bool>>,
    IRequestHandler<GetAllPromotionsQuery, Result<PagedResult<PromotionDto>>>,
    IRequestHandler<GetActivePromotionsQuery, Result<IEnumerable<PromotionDto>>>,
    IRequestHandler<GetPromotionByCodeQuery, Result<PromotionDto>>
{
    private readonly IRepository<TblPromotion> _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PromotionHandlers(
        IRepository<TblPromotion> promotionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _promotionRepository = promotionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PromotionDto>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = new TblPromotion
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            Name = request.Name,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinOrderAmount = request.MinOrderAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UsageLimit = request.UsageLimit,
            IsActive = true
        };

        await _promotionRepository.AddAsync(promotion, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<PromotionDto>(promotion));
    }

    public async Task<Result<PromotionDto>> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByCodeAsync(request.PromotionCode, cancellationToken);

        if (promotion == null)
            return Result.Failure<PromotionDto>(Error.NotFound("Promotion", request.PromotionCode));

        if (request.Name != null) promotion.Name = request.Name;
        if (request.Description != null) promotion.Description = request.Description;
        if (request.DiscountValue.HasValue) promotion.DiscountValue = request.DiscountValue.Value;
        if (request.MinOrderAmount.HasValue) promotion.MinOrderAmount = request.MinOrderAmount;
        if (request.MaxDiscountAmount.HasValue) promotion.MaxDiscountAmount = request.MaxDiscountAmount;
        if (request.StartDate.HasValue) promotion.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) promotion.EndDate = request.EndDate.Value;
        if (request.UsageLimit.HasValue) promotion.UsageLimit = request.UsageLimit;
        if (request.IsActive.HasValue) promotion.IsActive = request.IsActive;

        _promotionRepository.Update(promotion);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<PromotionDto>(promotion));
    }

    public async Task<Result<bool>> Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByCodeAsync(request.PromotionCode, cancellationToken);

        if (promotion == null)
            return Result.Failure<bool>(Error.NotFound("Promotion", request.PromotionCode));

        // Soft delete
        promotion.IsActive = false;
        _promotionRepository.Update(promotion);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<PagedResult<PromotionDto>>> Handle(GetAllPromotionsQuery request, CancellationToken cancellationToken)
    {
        var query = _promotionRepository.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.StartDate)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<PromotionDto>>(items);
        return Result.Success(new PagedResult<PromotionDto>(dtos, totalItems, request.PageIndex, request.PageSize));
    }

    public async Task<Result<IEnumerable<PromotionDto>>> Handle(GetActivePromotionsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var promotions = await _promotionRepository.AsQueryable()
            .Where(p => p.IsActive == true && p.StartDate <= now && p.EndDate >= now)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<IEnumerable<PromotionDto>>(promotions));
    }

    public async Task<Result<PromotionDto>> Handle(GetPromotionByCodeQuery request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByCodeAsync(request.PromotionCode, cancellationToken);

        if (promotion == null)
            return Result.Failure<PromotionDto>(Error.NotFound("Promotion", request.PromotionCode));

        return Result.Success(_mapper.Map<PromotionDto>(promotion));
    }
}
