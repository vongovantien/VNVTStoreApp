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

public class PromotionHandlers : BaseHandler<TblPromotion>,
    IRequestHandler<CreateCommand<CreatePromotionDto, PromotionDto>, Result<PromotionDto>>,
    IRequestHandler<UpdateCommand<UpdatePromotionDto, PromotionDto>, Result<PromotionDto>>,
    IRequestHandler<DeleteCommand<TblPromotion>, Result>,
    IRequestHandler<GetPagedQuery<PromotionDto>, Result<PagedResult<PromotionDto>>>,
    IRequestHandler<GetByCodeQuery<PromotionDto>, Result<PromotionDto>>,
    IRequestHandler<GetFlashSaleQuery, Result<List<PromotionDto>>>
{
    private readonly IRepository<TblProductPromotion> _productPromotionRepository;

    public PromotionHandlers(
        IRepository<TblPromotion> repository,
        IRepository<TblProductPromotion> productPromotionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(repository, unitOfWork, mapper, dapperContext)
    {
        _productPromotionRepository = productPromotionRepository;
    }

    public async Task<Result<PromotionDto>> Handle(CreateCommand<CreatePromotionDto, PromotionDto> request, CancellationToken cancellationToken)
    {
        var existing = await _repository.AsQueryable()
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

        await _repository.AddAsync(promotion, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        // Use MapToDto or _mapper.Map?
        // MapToDto was private. Let's use _mapper.Map
        return Result.Success(_mapper.Map<PromotionDto>(promotion));
    }

    public async Task<Result<PromotionDto>> Handle(UpdateCommand<UpdatePromotionDto, PromotionDto> request, CancellationToken cancellationToken)
    {
        var promotion = await _repository.AsQueryable()
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

        _repository.Update(promotion);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<PromotionDto>(promotion));
    }

    public async Task<Result> Handle(DeleteCommand<TblPromotion> request, CancellationToken cancellationToken)
    {
        var promotion = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (promotion == null) return Result.Failure(Error.NotFound("Promotion", request.Code));

        _repository.Delete(promotion);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PagedResult<PromotionDto>>> Handle(GetPagedQuery<PromotionDto> request, CancellationToken cancellationToken)
    {
        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "StartDate", SortDescending = request.SortDescending };

        // Ensure valid sort column map if needed, otherwise rely on DTO/Entity match
        if (sortDTO.SortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase)) 
            sortDTO.SortBy = "StartDate";

        return await GetPagedDapperAsync<PromotionDto>(
            request.PageIndex,
            request.PageSize,
            request.Searching,
            sortDTO,
            null, // referenceTables
            request.Fields,
            cancellationToken);
    }

    public async Task<Result<PromotionDto>> Handle(GetByCodeQuery<PromotionDto> request, CancellationToken cancellationToken)
    {
        var promotion = await _repository.AsQueryable()
            .Include(p => p.TblProductPromotions)
            .FirstOrDefaultAsync(p => p.Code == request.Code, cancellationToken);

        if (promotion == null) return Result.Failure<PromotionDto>(Error.NotFound("Promotion", request.Code));

        return Result.Success(_mapper.Map<PromotionDto>(promotion));
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
