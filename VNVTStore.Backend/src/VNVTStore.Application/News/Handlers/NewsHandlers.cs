using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.News.Handlers;

public class NewsHandlers : BaseHandler<TblNews>,
    IRequestHandler<CreateCommand<CreateNewsDto, NewsDto>, Result<NewsDto>>,
    IRequestHandler<UpdateCommand<UpdateNewsDto, NewsDto>, Result<NewsDto>>,
    IRequestHandler<DeleteCommand<TblNews>, Result>,
    IRequestHandler<GetAllQuery<NewsDto>, Result<IEnumerable<NewsDto>>>,
    IRequestHandler<GetByCodeQuery<NewsDto>, Result<NewsDto>>,
    IRequestHandler<GetPagedQuery<NewsDto>, Result<PagedResult<NewsDto>>>
{
    private readonly IFileService _fileService;

    public NewsHandlers(
        IRepository<TblNews> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IFileService fileService) : base(repository, unitOfWork, mapper, dapperContext)
    {
        _fileService = fileService;
    }

    public async Task<Result<NewsDto>> Handle(CreateCommand<CreateNewsDto, NewsDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = _mapper.Map<TblNews>(request.Dto);
            entity.Code = Guid.NewGuid().ToString("N").Substring(0, 10);
            entity.CreatedAt = DateTime.UtcNow;
            if (string.IsNullOrEmpty(entity.Slug))
            {
                entity.Slug = entity.Title.ToLower().Replace(" ", "-");
            }

            if (!string.IsNullOrEmpty(request.Dto.Thumbnail))
            {
                var saveResult = await _fileService.SaveAndLinkImagesAsync(
                    entity.Code,
                    "News",
                    new[] { request.Dto.Thumbnail },
                    "news",
                    cancellationToken);

                if (saveResult.IsSuccess)
                {
                    entity.Thumbnail = saveResult.Value.FirstOrDefault();
                }
            }

            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<NewsDto>(entity));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<NewsDto>> Handle(UpdateCommand<UpdateNewsDto, NewsDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = await _repository.GetByCodeAsync(request.Code, cancellationToken);
            if (entity == null) return Result.Failure<NewsDto>(Error.NotFound("News", request.Code));

            _mapper.Map(request.Dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.Dto.Thumbnail))
            {
                var saveResult = await _fileService.SaveAndLinkImagesAsync(
                    entity.Code,
                    "News",
                    new[] { request.Dto.Thumbnail },
                    "news",
                    cancellationToken);

                if (saveResult.IsSuccess)
                {
                    entity.Thumbnail = saveResult.Value.FirstOrDefault();
                }
            }

            _repository.Update(entity);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<NewsDto>(entity));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result> Handle(DeleteCommand<TblNews> request, CancellationToken cancellationToken)
    {
        var result = await DeleteAsync(request.Code, "News", cancellationToken);
        if (result.IsSuccess)
        {
            await _fileService.DeleteLinkedFilesAsync(request.Code, "News", cancellationToken);
        }
        return result;
    }

    public async Task<Result<IEnumerable<NewsDto>>> Handle(GetAllQuery<NewsDto> request, CancellationToken cancellationToken)
    {
        var entities = await _repository.AsQueryable()
            .Where(e => e.ModifiedType != ModificationType.Delete.ToString())
            .ToListAsync(cancellationToken);
        return Result.Success(_mapper.Map<IEnumerable<NewsDto>>(entities));
    }

    public async Task<Result<NewsDto>> Handle(GetByCodeQuery<NewsDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<NewsDto>(request.Code, "News", cancellationToken);
    }

    public async Task<Result<PagedResult<NewsDto>>> Handle(GetPagedQuery<NewsDto> request, CancellationToken cancellationToken)
    {
        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };

        return await GetPagedDapperAsync<NewsDto>(
            request.PageIndex,
            request.PageSize,
            request.Searching,
            sortDTO,
            null,
            request.Fields,
            cancellationToken);
    }
}
