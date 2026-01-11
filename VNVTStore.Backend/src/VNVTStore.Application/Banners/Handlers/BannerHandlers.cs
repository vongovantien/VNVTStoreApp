using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Banners.Handlers;

public class BannerHandlers : BaseHandler<TblBanner>,
    IRequestHandler<GetPagedQuery<BannerDto>, Result<PagedResult<BannerDto>>>,
    IRequestHandler<GetByCodeQuery<BannerDto>, Result<BannerDto>>,
    IRequestHandler<CreateCommand<CreateBannerDto, BannerDto>, Result<BannerDto>>,
    IRequestHandler<UpdateCommand<UpdateBannerDto, BannerDto>, Result<BannerDto>>,
    IRequestHandler<DeleteCommand<TblBanner>, Result>
{
    public BannerHandlers(IRepository<TblBanner> repository, IUnitOfWork unitOfWork, IMapper mapper) 
        : base(repository, unitOfWork, mapper)
    {
    }

    public async Task<Result<PagedResult<BannerDto>>> Handle(GetPagedQuery<BannerDto> request, CancellationToken cancellationToken)
    {
        return await GetPagedAsync<BannerDto>(
            request.PageIndex,
            request.PageSize,
            cancellationToken,
            predicate: p => string.IsNullOrWhiteSpace(request.Search) || p.Title.Contains(request.Search),
            orderBy: q => {
                var sortDto = request.SortDTO;
                if (sortDto != null && !string.IsNullOrWhiteSpace(sortDto.SortBy))
                {
                    return sortDto.SortBy.ToLower() switch
                    {
                        "priority" => sortDto.SortDescending ? q.OrderByDescending(p => p.Priority) : q.OrderBy(p => p.Priority),
                        "title" => sortDto.SortDescending ? q.OrderByDescending(p => p.Title) : q.OrderBy(p => p.Title),
                        "createdat" => sortDto.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt),
                        _ => q.OrderByDescending(p => p.CreatedAt)
                    };
                }
                return q.OrderByDescending(p => p.Priority).ThenByDescending(p => p.CreatedAt);
            });
    }

    public async Task<Result<BannerDto>> Handle(GetByCodeQuery<BannerDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<BannerDto>(request.Code, MessageConstants.Banner, cancellationToken);
    }

    public async Task<Result<BannerDto>> Handle(CreateCommand<CreateBannerDto, BannerDto> request, CancellationToken cancellationToken)
    {
        return await CreateAsync<CreateBannerDto, BannerDto>(
            request.Dto, 
            cancellationToken
        );
    }

    public async Task<Result<BannerDto>> Handle(UpdateCommand<UpdateBannerDto, BannerDto> request, CancellationToken cancellationToken)
    {
        var entity = await Repository.GetByCodeAsync(request.Code, cancellationToken);
        if (entity == null)
             return Result.Failure<BannerDto>(Error.NotFound(MessageConstants.Banner, request.Code));

        return await UpdateAsync<UpdateBannerDto, BannerDto>(
            request.Code, 
            request.Dto, 
            MessageConstants.Banner,
            cancellationToken
        );
    }

    public async Task<Result> Handle(DeleteCommand<TblBanner> request, CancellationToken cancellationToken)
    {
        return await DeleteAsync(request.Code, MessageConstants.Banner, cancellationToken);
    }
}
