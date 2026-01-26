using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Promotions.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Promotions.Handlers;

/// <summary>
/// Handler for specialized Promotion queries that are not covered by the generic BaseHandler.
/// </summary>
public class PromotionSpecialHandlers : 
    IRequestHandler<GetFlashSaleQuery, Result<List<PromotionDto>>>
{
    private readonly IRepository<TblPromotion> _repository;
    private readonly IMapper _mapper;

    public PromotionSpecialHandlers(IRepository<TblPromotion> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<PromotionDto>>> Handle(GetFlashSaleQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var promotions = await _repository.AsQueryable()
            .AsNoTracking()
            .Where(p => p.IsActive == true && p.StartDate <= now && p.EndDate >= now)
            .Where(p => p.TblProductPromotions.Any())
            .Select(p => new PromotionDto
            {
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                MinOrderAmount = p.MinOrderAmount,
                MaxDiscountAmount = p.MaxDiscountAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                UsageLimit = p.UsageLimit,
                IsActive = p.IsActive,
                ProductCodes = p.TblProductPromotions.Select(pp => pp.ProductCode).ToList()
            })
            .ToListAsync(cancellationToken);

        return Result.Success(promotions);
    }
}
