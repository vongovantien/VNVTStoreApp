using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Promotions.Queries;
using VNVTStore.Domain.Interfaces; // Added missing using
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Promotions.Handlers;

public class PromotionHandlers :
    IRequestHandler<CreateCommand<CreatePromotionDto, PromotionDto>, Result<PromotionDto>>,
    IRequestHandler<UpdateCommand<UpdatePromotionDto, PromotionDto>, Result<PromotionDto>>,
    IRequestHandler<DeleteCommand<TblPromotion>, Result>,
    IRequestHandler<GetPagedQuery<PromotionDto>, Result<PagedResult<PromotionDto>>>,
    IRequestHandler<GetByCodeQuery<PromotionDto>, Result<PromotionDto>>,
    IRequestHandler<GetFlashSaleQuery, Result<List<PromotionDto>>>
{
    private readonly IRepository<TblPromotion> _promotionRepository;
    private readonly IRepository<TblProductPromotion> _productPromotionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PromotionHandlers(
        IRepository<TblPromotion> promotionRepository,
        IRepository<TblProductPromotion> productPromotionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _promotionRepository = promotionRepository;
        _productPromotionRepository = productPromotionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PromotionDto>> Handle(CreateCommand<CreatePromotionDto, PromotionDto> request, CancellationToken cancellationToken)
    {
        var existing = await _promotionRepository.AsQueryable()
            .FirstOrDefaultAsync(p => p.Code == request.Dto.Code, cancellationToken);
        if (existing != null)
            return Result.Failure<PromotionDto>(Error.Validation("Promotion code already exists"));

        // Use AutoMapper to create entity
        var promotion = _mapper.Map<TblPromotion>(request.Dto);

        // Map Product Promotions manually as they need new IDs and specific logic
        // Or if AutoMapper supports it? For now, list Logic is fine here but main fields are mapped.
        // Actually, CreateMap<CreatePromotionDto, TblPromotion> maps fields. 
        // ProductCodes is not in TblPromotion, so ignored.
        
        // Manual Product Logic (Still needed as it's a child relationship logic)
        if (request.Dto.ProductCodes != null && request.Dto.ProductCodes.Any())
        {
            foreach (var pCode in request.Dto.ProductCodes)
            {
                promotion.TblProductPromotions.Add(new TblProductPromotion
                {
                    Code = Guid.NewGuid().ToString("N").Substring(0,10),
                    ProductCode = pCode,
                    PromotionCode = promotion.Code 
                });
            }
        }

        await _promotionRepository.AddAsync(promotion, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        // Use MapToDto or _mapper.Map?
        // MapToDto was private. Let's use _mapper.Map
        return Result.Success(_mapper.Map<PromotionDto>(promotion));
    }

    public async Task<Result<PromotionDto>> Handle(UpdateCommand<UpdatePromotionDto, PromotionDto> request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.AsQueryable()
            .Include(p => p.TblProductPromotions)
            .FirstOrDefaultAsync(p => p.Code == request.Code, cancellationToken);

        if (promotion == null)
            return Result.Failure<PromotionDto>(Error.NotFound("Promotion", request.Code));

        // Use AutoMapper to update fields
        _mapper.Map(request.Dto, promotion);

        // Sync Products
        if (request.Dto.ProductCodes != null)
        {
            var existingCodes = promotion.TblProductPromotions.Select(pp => pp.ProductCode).ToList();
            var newCodes = request.Dto.ProductCodes;

            var toRemove = promotion.TblProductPromotions.Where(pp => !newCodes.Contains(pp.ProductCode)).ToList();
            if (toRemove.Any())
            {
                _productPromotionRepository.DeleteRange(toRemove);
            }

            var toAdd = newCodes.Where(c => !existingCodes.Contains(c)).ToList();
            if (toAdd.Any())
            {
                var newItems = toAdd.Select(code => new TblProductPromotion
                {
                    Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                    ProductCode = code,
                    PromotionCode = promotion.Code
                }).ToList();
                await _productPromotionRepository.AddRangeAsync(newItems, cancellationToken);
            }
        }

        _promotionRepository.Update(promotion);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<PromotionDto>(promotion));
    }

    public async Task<Result> Handle(DeleteCommand<TblPromotion> request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (promotion == null) return Result.Failure(Error.NotFound("Promotion", request.Code));

        _promotionRepository.Delete(promotion);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PagedResult<PromotionDto>>> Handle(GetPagedQuery<PromotionDto> request, CancellationToken cancellationToken)
    {
        var query = _promotionRepository.AsQueryable();

        // Handle generic filters
        if (request.Filters != null)
        {
             query = QueryHelper.ApplyFilters(query, request.Filters);
        }
        
        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(p => p.Name.Contains(request.Search) || p.Code.Contains(request.Search));
        }

        var total = await query.CountAsync(cancellationToken);
        
        List<PromotionDto> dtos;

        if (request.Fields != null && request.Fields.Any())
        {
            query = query
                .OrderByDescending(p => p.StartDate)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking();
                
            query = QueryHelper.ApplySelection(query, request.Fields);
            
            var items = await query.ToListAsync(cancellationToken);
            dtos = _mapper.Map<List<PromotionDto>>(items);
        }
        else
        {
            dtos = await query
                .OrderByDescending(p => p.StartDate)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking() // Performance optimization
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
        }

        return Result.Success(new PagedResult<PromotionDto>(dtos, total));
    }

    public async Task<Result<PromotionDto>> Handle(GetByCodeQuery<PromotionDto> request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.AsQueryable()
            .Include(p => p.TblProductPromotions)
            .FirstOrDefaultAsync(p => p.Code == request.Code, cancellationToken);

        if (promotion == null) return Result.Failure<PromotionDto>(Error.NotFound("Promotion", request.Code));

        return Result.Success(_mapper.Map<PromotionDto>(promotion));
    }

    public async Task<Result<List<PromotionDto>>> Handle(GetFlashSaleQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var promotions = await _promotionRepository.AsQueryable()
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
