using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Promotions.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Promotions.Handlers;

public class PromotionHandlers : BaseHandler<TblPromotion>,
    IRequestHandler<CreateCommand<CreatePromotionDto, PromotionDto>, Result<PromotionDto>>,
    IRequestHandler<UpdateCommand<UpdatePromotionDto, PromotionDto>, Result<PromotionDto>>,
    IRequestHandler<DeleteCommand<TblPromotion>, Result>,
    IRequestHandler<GetPagedQuery<PromotionDto>, Result<PagedResult<PromotionDto>>>,
    IRequestHandler<GetActivePromotionsQuery, Result<IEnumerable<PromotionDto>>>,
    IRequestHandler<GetByCodeQuery<PromotionDto>, Result<PromotionDto>>
{
    public PromotionHandlers(
        IRepository<TblPromotion> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(repository, unitOfWork, mapper)
    {
    }

    public async Task<Result<PromotionDto>> Handle(CreateCommand<CreatePromotionDto, PromotionDto> request, CancellationToken cancellationToken)
    {
        return await CreateAsync<CreatePromotionDto, PromotionDto>(
            request.Dto,
            cancellationToken,
            p => {
                p.Code = Guid.NewGuid().ToString("N").Substring(0, 10);
                p.IsActive = true;
            });
    }

    public async Task<Result<PromotionDto>> Handle(UpdateCommand<UpdatePromotionDto, PromotionDto> request, CancellationToken cancellationToken)
    {
        return await UpdateAsync<UpdatePromotionDto, PromotionDto>(
            request.Code,
            request.Dto,
            MessageConstants.Coupon, // Using Coupon as entity name for localization as seen in previous handler
            cancellationToken);
    }

    public async Task<Result> Handle(DeleteCommand<TblPromotion> request, CancellationToken cancellationToken)
    {
        return await DeleteAsync(request.Code, MessageConstants.Coupon, cancellationToken);
    }

    public async Task<Result<PagedResult<PromotionDto>>> Handle(GetPagedQuery<PromotionDto> request, CancellationToken cancellationToken)
    {
        return await GetPagedAsync<PromotionDto>(
            request.PageIndex,
            request.PageSize,
            cancellationToken,
            predicate: p => true, // Simplified for now as GetPagedQuery doesn't have IsActive. Or cast request to custom query?
            orderBy: q => q.OrderByDescending(p => p.StartDate));
    }

    public async Task<Result<IEnumerable<PromotionDto>>> Handle(GetActivePromotionsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var promotions = await Repository.AsQueryable()
            .Where(p => p.IsActive == true && p.StartDate <= now && p.EndDate >= now)
            .ToListAsync(cancellationToken);

        return Result.Success(Mapper.Map<IEnumerable<PromotionDto>>(promotions));
    }

    public async Task<Result<PromotionDto>> Handle(GetByCodeQuery<PromotionDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<PromotionDto>(request.Code, MessageConstants.Coupon, cancellationToken);
    }
}
