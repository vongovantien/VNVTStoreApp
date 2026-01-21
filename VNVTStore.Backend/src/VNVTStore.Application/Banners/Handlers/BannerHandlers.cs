#nullable disable
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Interfaces;

using Dapper;
using VNVTStore.Application.Common.Helpers;
using Newtonsoft.Json.Linq;

namespace VNVTStore.Application.Banners.Handlers;

public class BannerHandlers : BaseHandler<TblBanner>,
    IRequestHandler<GetPagedQuery<BannerDto>, Result<PagedResult<BannerDto>>>,
    IRequestHandler<GetByCodeQuery<BannerDto>, Result<BannerDto>>,
    IRequestHandler<CreateCommand<CreateBannerDto, BannerDto>, Result<BannerDto>>,
    IRequestHandler<UpdateCommand<UpdateBannerDto, BannerDto>, Result<BannerDto>>,
    IRequestHandler<DeleteCommand<TblBanner>, Result>
{
    private readonly IImageUploadService _imageUploadService;
    private readonly IApplicationDbContext _context;


    public BannerHandlers(IRepository<TblBanner> repository, IUnitOfWork unitOfWork, IMapper mapper, IImageUploadService imageUploadService, IApplicationDbContext context, IDapperContext dapperContext) 
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _imageUploadService = imageUploadService;
        _context = context;
    }

    public async Task<Result<PagedResult<BannerDto>>> Handle(GetPagedQuery<BannerDto> request, CancellationToken cancellationToken)
    {
        var searchFields = request.Searching ?? new List<SearchDTO>();

        searchFields.Add(new SearchDTO { SearchField = "Title", SearchCondition = SearchCondition.Contains, SearchValue = request.Search });

        var result = await GetPagedDapperAsync<BannerDto>(request.PageIndex, request.PageSize, searchFields, request.SortDTO, null, request.Fields, cancellationToken);
        
        // Populate Images Logic (since GetPagedDapperAsync doesn't handle child collections yet)
        if (result.IsSuccess && result.Value.Items.Any())
        {
             using var connection = _dapperContext.CreateConnection();
             var bannerCodes = result.Value.Items.Select(b => b.Code).ToList();
             var fileSql = @"SELECT * FROM ""TblFile"" WHERE ""MasterCode"" = ANY(@Codes) AND ""MasterType"" = 'Banner'";
             var files = await connection.QueryAsync<TblFile>(fileSql, new { Codes = bannerCodes });
            
             foreach (var item in result.Value.Items)
             {
                var file = files.FirstOrDefault(f => f.MasterCode == item.Code);
                if (file != null)
                {
                    item.ImageUrl = file.Path; 
                }
             }
        }
        
        return result;
    }

    public async Task<Result<BannerDto>> Handle(GetByCodeQuery<BannerDto> request, CancellationToken cancellationToken)
    {
        var result = await GetByCodeAsync<BannerDto>(request.Code, MessageConstants.Banner, cancellationToken);
        if (result.IsSuccess)
        {
            var file = await _context.TblFiles.FirstOrDefaultAsync(f => f.MasterCode == request.Code && f.MasterType == "Banner", cancellationToken);
            if (file != null)
            {
                result.Value.ImageUrl = file.Path;
            }
        }
        return result;
    }

    public async Task<Result<BannerDto>> Handle(CreateCommand<CreateBannerDto, BannerDto> request, CancellationToken cancellationToken)
    {
        string? uploadedPath = null;
        if (!string.IsNullOrEmpty(request.Dto.ImageUrl) && request.Dto.ImageUrl.StartsWith("data:image"))
        {
            var fileName = $"banner_{DateTime.Now.Ticks}.png"; 
            var uploadResult = await _imageUploadService.UploadBase64Async(request.Dto.ImageUrl, fileName, "banners");
            if (uploadResult.IsFailure)
            {
                return Result.Failure<BannerDto>(uploadResult.Error);
            }
            uploadedPath = uploadResult.Value.Url;
            // request.Dto.ImageUrl = uploadedPath; // Dto ImageUrl is transient here
        }

        var result = await CreateAsync<CreateBannerDto, BannerDto>(
            request.Dto, 
            cancellationToken,
             c => {
                if (string.IsNullOrEmpty(c.Code))
                {
                    c.Code = $"BNN{DateTime.Now.Ticks.ToString().Substring(12)}";
                }
                c.IsActive = request.Dto.IsActive;
            }
        );

        if (result.IsSuccess && !string.IsNullOrEmpty(uploadedPath))
        {
            var file = await _context.TblFiles.FirstOrDefaultAsync(f => f.Path == uploadedPath, cancellationToken);
            if (file != null)
            {
                file.MasterCode = result.Value.Code;
                file.MasterType = "Banner";
                await _context.SaveChangesAsync(cancellationToken); 
            }
            result.Value.ImageUrl = uploadedPath;
        }

        return result;
    }

    public async Task<Result<BannerDto>> Handle(UpdateCommand<UpdateBannerDto, BannerDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try 
        {
            // 1. Upload Logic
            string? uploadedPath = null;
            if (!string.IsNullOrEmpty(request.Dto.ImageUrl) && request.Dto.ImageUrl.StartsWith("data:image"))
            {
                var fileName = $"banner_{DateTime.Now.Ticks}.png"; 
                var uploadResult = await _imageUploadService.UploadBase64Async(request.Dto.ImageUrl, fileName, "banners");
                if (uploadResult.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BannerDto>(uploadResult.Error);
                }
                uploadedPath = uploadResult.Value.Url;
            }
            else if (!string.IsNullOrEmpty(request.Dto.ImageUrl))
            {
                // Kept existing image
                uploadedPath = request.Dto.ImageUrl;
            }

            var entity = await _repository.GetByCodeAsync(request.Code, cancellationToken);
            if (entity == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BannerDto>(Error.NotFound(MessageConstants.Banner, request.Code));
            }

            _mapper.Map(request.Dto, entity);
            _repository.Update(entity);

            // Handle Files
            if (uploadedPath != null)
            {
                // If it's a new upload (checked by logic or assuming if passed and valid)
                // Actually if it matches existing, we do nothing.
                // But if it is new (was base64), we link it and unlink old.
                
                // Get current file
                var currentFile = await _context.TblFiles.FirstOrDefaultAsync(f => f.MasterCode == request.Code && f.MasterType == "Banner", cancellationToken);
                
                if (currentFile != null && currentFile.Path != uploadedPath)
                {
                     // Remove old file link or delete
                     _context.TblFiles.Remove(currentFile);
                     // Strictly delete from cloud if we are replacing it
                     if (!string.IsNullOrEmpty(currentFile.Path))
                     {
                         await _imageUploadService.DeleteImagesAsync(new[] { currentFile.Path });
                     }
                }

                if (currentFile == null || currentFile.Path != uploadedPath)
                {
                    var newFile = await _context.TblFiles.FirstOrDefaultAsync(f => f.Path == uploadedPath, cancellationToken);
                    if (newFile != null)
                    {
                        newFile.MasterCode = entity.Code;
                        newFile.MasterType = "Banner";
                    }
                }
            }
             else if (request.Dto.ImageUrl == "") // Explicitly removed
            {
                var currentFile = await _context.TblFiles.FirstOrDefaultAsync(f => f.MasterCode == request.Code && f.MasterType == "Banner", cancellationToken);
                if (currentFile != null) _context.TblFiles.Remove(currentFile);
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var resultDto = _mapper.Map<BannerDto>(entity);
            resultDto.ImageUrl = uploadedPath;
            return Result.Success(resultDto);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result> Handle(DeleteCommand<TblBanner> request, CancellationToken cancellationToken)
    {
        return await DeleteAsync(request.Code, MessageConstants.Banner, cancellationToken);
    }
}
