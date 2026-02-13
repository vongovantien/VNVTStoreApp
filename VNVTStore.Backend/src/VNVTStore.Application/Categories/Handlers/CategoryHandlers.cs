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

namespace VNVTStore.Application.Categories.Handlers;

public class CategoryHandlers : BaseHandler<TblCategory>,
    IRequestHandler<CreateCommand<CreateCategoryDto, CategoryDto>, Result<CategoryDto>>,
    IRequestHandler<UpdateCommand<UpdateCategoryDto, CategoryDto>, Result<CategoryDto>>,
    IRequestHandler<DeleteCommand<TblCategory>, Result>,
    IRequestHandler<GetByCodeQuery<CategoryDto>, Result<CategoryDto>>,
    IRequestHandler<DeleteMultipleCommand<TblCategory>, Result>,
    IRequestHandler<GetStatsQuery<TblCategory>, Result<EntityStatsDto>>
{
     private readonly IFileService _fileService;

    public CategoryHandlers(
        IRepository<TblCategory> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IFileService fileService) : base(repository, unitOfWork, mapper, dapperContext)
    {
        _fileService = fileService;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCommand<CreateCategoryDto, CategoryDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = _mapper.Map<TblCategory>(request.Dto);
            
            // Manual code gen if not relied on DB default for files
            entity.Code = "CAT" + Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            entity.ModifiedType = "ADD";

            await _repository.AddAsync(entity, cancellationToken);
            
            // Handle Images
             if (!string.IsNullOrEmpty(request.Dto.ImageUrl))
            {
                var saveResult = await _fileService.SaveAndLinkImagesAsync(
                    entity.Code,
                    "CATEGORY", 
                    new[] { request.Dto.ImageUrl },
                    "categories",
                    cancellationToken);
                
                // We don't save back to TblCategory.ImageUrl as column is removed/deprecated
                // TblFile link is enough.
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<CategoryDto>(entity));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCommand<UpdateCategoryDto, CategoryDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = await _repository.GetByCodeAsync(request.Code, cancellationToken);
            if (entity == null) return Result.Failure<CategoryDto>(Error.NotFound("Category", request.Code));

            _mapper.Map(request.Dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.ModifiedType = "UPDATE";

            if (!string.IsNullOrEmpty(request.Dto.ImageUrl))
            {
                var saveResult = await _fileService.SaveAndLinkImagesAsync(
                    entity.Code,
                    "CATEGORY",
                    new[] { request.Dto.ImageUrl },
                    "categories",
                    cancellationToken);
            }

            _repository.Update(entity);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<CategoryDto>(entity));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result> Handle(DeleteCommand<TblCategory> request, CancellationToken cancellationToken)
    {
         // Check Products
         using var connection = _dapperContext.CreateConnection();
         var hasProducts = await SqlMapper.ExecuteScalarAsync<bool>(
            connection,
            "SELECT EXISTS(SELECT 1 FROM \"TblProduct\" WHERE \"CategoryCode\" = @Code AND \"ModifiedType\" != 'DELETE')", 
            new { Code = request.Code });

        if (hasProducts)
        {
            return Result.Failure(Error.Conflict(MessageConstants.Conflict, "Cannot delete Category because it has associated Products."));
        }
        
        // Check Subcategories
         var hasChildren = await _repository.AsQueryable().AnyAsync(c => c.ParentCode == request.Code && c.ModifiedType != "DELETE", cancellationToken);
         if (hasChildren)
            return Result.Failure(Error.Conflict(MessageConstants.Conflict, "Cannot delete Category because it has Subcategories."));


        var result = await DeleteAsync(request.Code, "Category", cancellationToken);
        if (result.IsSuccess)
        {
            await _fileService.DeleteLinkedFilesAsync(request.Code, "CATEGORY", cancellationToken);
        }
        return result;
    }

    public async Task<Result<CategoryDto>> Handle(GetByCodeQuery<CategoryDto> request, CancellationToken cancellationToken)
    {
        // Get Basic Info
        var result = await GetByCodeAsync<CategoryDto>(request.Code, "Category", cancellationToken);
        if (result.IsSuccess)
        {
            // Populate File manually via query? 
            // Or let DTO ReferenceCollection handle it if GetByCodeAsync supports it?
            // BaseHandler GetByCodeAsync doesn't automatically loop invalid includes logic.
            // But we can use IFileService to fetch?
            // Or we just return DTO. If FE needs files, it might use GetPaged logic or we enhance this.
            // Let's enhance it quickly.
            
            // Actually, we can just query files using DapperContext and map.
             using var connection = _dapperContext.CreateConnection();
             var files = await SqlMapper.QueryAsync<string>(
                connection,
                "SELECT \"Path\" FROM \"TblFile\" WHERE \"MasterCode\" = @Code AND \"MasterType\" = 'CATEGORY'", 
                new { Code = request.Code });
            
            if (files.Any())
            {
                result.Value.ImageUrl = files.First(); // Or list
            }
        }
        return result;
    }
    
    public async Task<Result> Handle(DeleteMultipleCommand<TblCategory> request, CancellationToken cancellationToken)
    {
        return await DeleteMultipleAsync(request.Codes, "Category", cancellationToken);
    }

    public async Task<Result<EntityStatsDto>> Handle(GetStatsQuery<TblCategory> request, CancellationToken cancellationToken)
    {
         var count = await _repository.AsQueryable().CountAsync(cancellationToken);
         // Fixed: TotalCount -> Total, ActiveCount -> Active
         return Result.Success(new EntityStatsDto { Total = count, Active = count });
    }
}
