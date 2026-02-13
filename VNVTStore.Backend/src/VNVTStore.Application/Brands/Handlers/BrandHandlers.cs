using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace VNVTStore.Application.Brands.Handlers;

public class BrandHandlers : BaseHandler<TblBrand>,
    IRequestHandler<CreateCommand<CreateBrandDto, BrandDto>, Result<BrandDto>>,
    IRequestHandler<UpdateCommand<UpdateBrandDto, BrandDto>, Result<BrandDto>>,
    IRequestHandler<DeleteCommand<TblBrand>, Result>,
    IRequestHandler<GetByCodeQuery<BrandDto>, Result<BrandDto>>,
    IRequestHandler<GetPagedQuery<BrandDto>, Result<PagedResult<BrandDto>>>,
    IRequestHandler<DeleteMultipleCommand<TblBrand>, Result>,
    IRequestHandler<GetStatsQuery<TblBrand>, Result<EntityStatsDto>>
{
    private readonly IFileService _fileService;

    public BrandHandlers(
        IRepository<TblBrand> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IFileService fileService) : base(repository, unitOfWork, mapper, dapperContext)
    {
        _fileService = fileService;
    }

    public async Task<Result<BrandDto>> Handle(CreateCommand<CreateBrandDto, BrandDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = _mapper.Map<TblBrand>(request.Dto);
            
            // Generate Code if not present (handled by DB default usually, but we can generate if needed or rely on sequence)
            // For TblBrand, default is BRN + sequence. BaseHandler/Repo usually handles add.
            // But we need the Code for File linking if we do it before save? 
            // Actually, for TblBrand, code generation is DB side default.
            // But we need the ID to link files. 
            // If we assume default value sql handles it, we get the ID back after AddAsync + Commit?
            // Use Guid driven code if manually generating, or let DB handle it.
            // Check TblBrand configuration: HasDefaultValueSql ... ('BRN'...)
            
            // In NewsHandler, it generated code manually: Guid....
            // Let's generate manually to be safe and consistent with File Linking which needs code.
            entity.Code = "BRN" + Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();
            
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.IsActive = true; 
            entity.ModifiedType = "ADD";

            if (!string.IsNullOrEmpty(request.Dto.LogoUrl))
            {
                var saveResult = await _fileService.SaveAndLinkImagesAsync(
                    entity.Code,
                    "BRAND", // MasterType
                    new[] { request.Dto.LogoUrl },
                    "brands",
                    cancellationToken);

                if (saveResult.IsSuccess)
                {
                    entity.LogoUrl = saveResult.Value.FirstOrDefault();
                }
            }

            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<BrandDto>(entity));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<BrandDto>> Handle(UpdateCommand<UpdateBrandDto, BrandDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = await _repository.GetByCodeAsync(request.Code, cancellationToken);
            if (entity == null) return Result.Failure<BrandDto>(Error.NotFound("Brand", request.Code));

            _mapper.Map(request.Dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.ModifiedType = "UPDATE";

            if (!string.IsNullOrEmpty(request.Dto.LogoUrl))
            {
                // Logic: If new URL is provided, save it. 
                // Note: Frontend might send existing URL. Check if it's base64/new file or existing path?
                // SaveAndLinkImagesAsync usually handles detection? 
                // Or we assume validation/logic is in Service.
                
                var saveResult = await _fileService.SaveAndLinkImagesAsync(
                    entity.Code,
                    "BRAND",
                    new[] { request.Dto.LogoUrl },
                    "brands",
                    cancellationToken);

                if (saveResult.IsSuccess && saveResult.Value.Any())
                {
                    entity.LogoUrl = saveResult.Value.FirstOrDefault();
                }
            }

            _repository.Update(entity);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<BrandDto>(entity));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result> Handle(DeleteCommand<TblBrand> request, CancellationToken cancellationToken)
    {
        // Check dependencies (Products)
        // Fix: Use SqlMapper or Ensure Dapper namespace is used for extensions
        // But better to use SqlMapper.ExecuteScalarAsync
        using var connection = _dapperContext.CreateConnection();
        var hasProducts = await SqlMapper.ExecuteScalarAsync<bool>(
            connection,
            "SELECT EXISTS(SELECT 1 FROM \"TblProduct\" WHERE \"BrandCode\" = @Code AND \"ModifiedType\" != 'DELETE')", 
            new { Code = request.Code });

        if (hasProducts)
        {
            return Result.Failure(Error.Conflict(MessageConstants.Conflict, "Cannot delete Brand because it has associated Products."));
        }

        var result = await DeleteAsync(request.Code, "Brand", cancellationToken);
        if (result.IsSuccess)
        {
            await _fileService.DeleteLinkedFilesAsync(request.Code, "BRAND", cancellationToken);
        }
        return result;
    }

    public async Task<Result<BrandDto>> Handle(GetByCodeQuery<BrandDto> request, CancellationToken cancellationToken)
    {
        var result = await GetByCodeAsync<BrandDto>(request.Code, "Brand", cancellationToken);
        // ...
        return result;
    }

    public async Task<Result<PagedResult<BrandDto>>> Handle(GetPagedQuery<BrandDto> request, CancellationToken cancellationToken)
    {
        return await GetPagedDapperAsync<BrandDto>(
            request.PageIndex,
            request.PageSize,
            request.Searching,
            request.SortDTO,
            null,
            request.Fields,
            cancellationToken);
    }

    public async Task<Result> Handle(DeleteMultipleCommand<TblBrand> request, CancellationToken cancellationToken)
    {
         return await DeleteMultipleAsync(request.Codes, "Brand", cancellationToken);
    }
    
    public async Task<Result<EntityStatsDto>> Handle(GetStatsQuery<TblBrand> request, CancellationToken cancellationToken)
    {
         var count = await _repository.AsQueryable().CountAsync(cancellationToken);
         // Fixed: TotalCount -> Total, ActiveCount -> Active
         return Result.Success(new EntityStatsDto { Total = count, Active = count }); 
    }
}
