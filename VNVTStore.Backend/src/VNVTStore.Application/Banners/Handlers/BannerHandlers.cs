using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Dapper;
using System.Data;

namespace VNVTStore.Application.Banners.Handlers;

public class CreateBannerHandler : BaseHandler<TblBanner>,
    IRequestHandler<CreateCommand<CreateBannerDto, BannerDto>, Result<BannerDto>>
{
    private readonly IFileService _fileService;
    private readonly IBaseUrlService _baseUrlService;

    public CreateBannerHandler(
        IRepository<TblBanner> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IFileService fileService,
        IBaseUrlService baseUrlService)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _fileService = fileService;
        _baseUrlService = baseUrlService;
    }

    public async Task<Result<BannerDto>> Handle(CreateCommand<CreateBannerDto, BannerDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var dto = request.Dto;
            var banner = _mapper.Map<TblBanner>(dto);

            if (string.IsNullOrEmpty(banner.Code))
            {
                banner.Code = $"BNN{DateTime.Now.Ticks.ToString().Substring(12)}";
            }

            // Handle Image Upload if base64 via TblFile
            string? savedLocalPath = null;
            if (!string.IsNullOrEmpty(dto.ImageURL) && dto.ImageURL.StartsWith("data:image"))
            {
                var uploadResult = await _fileService.SaveAndLinkImagesAsync(
                    banner.Code,
                    "Banner",
                    new[] { dto.ImageURL },
                    "banners",
                    cancellationToken);

                if (uploadResult.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BannerDto>(uploadResult.Error);
                }
                
                savedLocalPath = uploadResult.Value.FirstOrDefault();
            }

            await _repository.AddAsync(banner, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var resultDto = _mapper.Map<BannerDto>(banner);
            
            // Resolve full URL from TblFile or directly if it was already a URL
            if (savedLocalPath != null)
            {
                resultDto.ImageURL = savedLocalPath.StartsWith("http") ? savedLocalPath : $"{_baseUrlService.GetBaseUrl().TrimEnd('/')}/{savedLocalPath.TrimStart('/')}";
            }
            else
            {
                resultDto.ImageURL = dto.ImageURL; // Keep original if it wasn't base64
            }

            return Result.Success(resultDto);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public class UpdateBannerHandler : BaseHandler<TblBanner>,
    IRequestHandler<UpdateCommand<UpdateBannerDto, BannerDto>, Result<BannerDto>>
{
    private readonly IFileService _fileService;
    private readonly IBaseUrlService _baseUrlService;

    public UpdateBannerHandler(
        IRepository<TblBanner> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IFileService fileService,
        IBaseUrlService baseUrlService)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _fileService = fileService;
        _baseUrlService = baseUrlService;
    }

    public async Task<Result<BannerDto>> Handle(UpdateCommand<UpdateBannerDto, BannerDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var banner = await _repository.GetByCodeAsync(request.Code, cancellationToken);
            if (banner == null)
                return Result.Failure<BannerDto>(Error.NotFound("Banner", request.Code));

            var dto = request.Dto;

            string? currentPath = null;
            if (!string.IsNullOrEmpty(dto.ImageURL) && dto.ImageURL.StartsWith("data:image"))
            {
                var uploadResult = await _fileService.SaveAndLinkImagesAsync(
                    banner.Code,
                    "Banner",
                    new[] { dto.ImageURL },
                    "banners",
                    cancellationToken);

                if (uploadResult.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BannerDto>(uploadResult.Error);
                }
                currentPath = uploadResult.Value.FirstOrDefault();
            }
            else
            {
                currentPath = dto.ImageURL;
            }

            banner.Update(
                dto.Title ?? banner.Title,
                dto.Content ?? banner.Content,
                dto.LinkUrl ?? banner.LinkUrl,
                dto.LinkText ?? banner.LinkText,
                dto.IsActive ?? banner.IsActive,
                dto.Priority ?? banner.Priority
            );

            _repository.Update(banner);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var resultDto = _mapper.Map<BannerDto>(banner);
            if (!string.IsNullOrEmpty(currentPath))
            {
                resultDto.ImageURL = currentPath.StartsWith("http") ? currentPath : $"{_baseUrlService.GetBaseUrl().TrimEnd('/')}/{currentPath.TrimStart('/')}";
            }

            return Result.Success(resultDto);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public class GetBannersHandler : BaseHandler<TblBanner>,
    IRequestHandler<GetPagedQuery<BannerDto>, Result<PagedResult<BannerDto>>>
{
    private readonly IBaseUrlService _baseUrlService;

    public GetBannersHandler(
        IRepository<TblBanner> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IBaseUrlService baseUrlService)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _baseUrlService = baseUrlService;
    }

    public async Task<Result<PagedResult<BannerDto>>> Handle(GetPagedQuery<BannerDto> request, CancellationToken cancellationToken)
    {
        var result = await GetPagedDapperAsync<BannerDto>(request.PageIndex, request.PageSize, request.Searching, request.SortDTO, null, request.Fields, cancellationToken);

        if (result.IsSuccess && result.Value.Items.Any())
        {
            using var connection = _dapperContext.CreateConnection();
            var bannerCodes = result.Value.Items.Select(p => p.Code).ToList();
            
            var sql = "SELECT * FROM \"TblFile\" WHERE \"MasterCode\" = ANY(@Codes) AND \"MasterType\" ILIKE 'Banner'";
            var files = (await SqlMapper.QueryAsync<TblFile>(connection, sql, new { Codes = bannerCodes.ToArray() })).ToList();
            
            var fileMap = files
                .GroupBy(f => f.MasterCode)
                .ToDictionary(g => g.Key!, g => g.FirstOrDefault()?.Path);
            
            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');

            foreach (var dto in result.Value.Items)
            {
                if (fileMap.TryGetValue(dto.Code, out var path) && !string.IsNullOrEmpty(path))
                {
                    dto.ImageURL = path.StartsWith("http") ? path : $"{baseUrl}/{path.TrimStart('/')}";
                }
            }
        }

        return result;
    }
}

public class GetBannerByCodeHandler : BaseHandler<TblBanner>,
    IRequestHandler<GetByCodeQuery<BannerDto>, Result<BannerDto>>
{
    private readonly IBaseUrlService _baseUrlService;

    public GetBannerByCodeHandler(
        IRepository<TblBanner> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IBaseUrlService baseUrlService)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _baseUrlService = baseUrlService;
    }

    public async Task<Result<BannerDto>> Handle(GetByCodeQuery<BannerDto> request, CancellationToken cancellationToken)
    {
        var banner = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (banner == null)
            return Result.Failure<BannerDto>(Error.NotFound("Banner", request.Code));

        var dto = _mapper.Map<BannerDto>(banner);
        
        using var connection = _dapperContext.CreateConnection();
        var sql = "SELECT \"Path\" FROM \"TblFile\" WHERE \"MasterCode\" = @Code AND \"MasterType\" ILIKE 'Banner' LIMIT 1";
        var path = await SqlMapper.QueryFirstOrDefaultAsync<string>(connection, sql, new { Code = request.Code });
        
        if (!string.IsNullOrEmpty(path))
        {
            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');
            dto.ImageURL = path.StartsWith("http") ? path : $"{baseUrl}/{path.TrimStart('/')}";
        }

        return Result.Success(dto);
    }
}
