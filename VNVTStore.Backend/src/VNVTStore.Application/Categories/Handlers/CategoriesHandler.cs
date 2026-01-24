#nullable disable
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VNVTStore.Application.Categories.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;

using Dapper;
using VNVTStore.Application.Common.Helpers;
using Newtonsoft.Json.Linq;

namespace VNVTStore.Application.Categories.Handlers;

public class CategoriesHandler : BaseHandler<TblCategory>,
    IRequestHandler<GetCategoriesQuery, Result<PagedResult<CategoryDto>>>,
    IRequestHandler<CreateCommand<CreateCategoryDto, CategoryDto>, Result<CategoryDto>>,
    IRequestHandler<UpdateCommand<UpdateCategoryDto, CategoryDto>, Result<CategoryDto>>,
    IRequestHandler<DeleteCommand<TblCategory>, Result>,
    IRequestHandler<DeleteMultipleCommand<TblCategory>, Result>,
    IRequestHandler<GetByCodeQuery<CategoryDto>, Result<CategoryDto>>,
    IRequestHandler<GetCategoryStatsQuery, Result<CategoryStatsDto>>
{
    private readonly IImageUploadService _imageUploadService;
    private readonly IApplicationDbContext _context; 
    private readonly IRepository<TblProduct> _productRepository;

    public CategoriesHandler(
        IRepository<TblCategory> repository,
        IRepository<TblProduct> productRepository,
        IImageUploadService imageUploadService,
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _productRepository = productRepository;
        _imageUploadService = imageUploadService;
        _context = context;
    }

    public async Task<Result<PagedResult<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var searchFields = request.Searching ?? new List<SearchDTO>();
        if (!string.IsNullOrEmpty(request.Search))
        {
             searchFields.Add(new SearchDTO { SearchField = "Name", SearchCondition = SearchCondition.Contains, SearchValue = request.Search });
        }
        
        // Use default sort if not provided
        var sort = request.SortDTO ?? new SortDTO { SortBy = "CreatedAt", SortDescending = true };

        return await GetPagedDapperAsync<CategoryDto>(
            request.PageIndex, 
            request.PageSize, 
            searchFields, 
            sort, 
            null, 
            request.Fields, 
            cancellationToken);
    }


    
// Corrected implementation:
    public async Task<Result<CategoryDto>> Handle(CreateCommand<CreateCategoryDto, CategoryDto> request, CancellationToken cancellationToken)
    {
        // Sanitize ParentCode: Convert empty string to null to avoid FK violation
        if (string.IsNullOrWhiteSpace(request.Dto.ParentCode))
        {
            request.Dto.ParentCode = null;
        }

        string? uploadedPath = null;
        if (!string.IsNullOrEmpty(request.Dto.ImageURL) && request.Dto.ImageURL.StartsWith("data:image"))
        {
            var fileName = $"category_{DateTime.Now.Ticks}.png"; 
            var uploadResult = await _imageUploadService.UploadBase64Async(request.Dto.ImageURL, fileName, "categories");
            if (uploadResult.IsFailure)
            {
                return Result.Failure<CategoryDto>(uploadResult.Error);
            }
            uploadedPath = uploadResult.Value.Url;
            request.Dto.ImageURL = uploadedPath;
        }

        var result = await CreateAsync<CreateCategoryDto, CategoryDto>(
            request.Dto,
            cancellationToken,
            c => {
                if (string.IsNullOrEmpty(c.Code))
                {
                    c.Code = $"CAT{DateTime.Now.Ticks.ToString().Substring(12)}";
                }
                c.IsActive = true;
            });

        if (result.IsSuccess && !string.IsNullOrEmpty(uploadedPath))
        {
            var file = await _context.TblFiles.FirstOrDefaultAsync(f => f.Path == uploadedPath, cancellationToken);
            if (file != null)
            {
                file.MasterCode = result.Value.Code;
                file.MasterType = "Category";
                await _context.SaveChangesAsync(cancellationToken); 
            }
        }
        return result;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCommand<UpdateCategoryDto, CategoryDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Sanitize ParentCode
            if (string.IsNullOrWhiteSpace(request.Dto.ParentCode))
            {
                request.Dto.ParentCode = null;
            }

            // 1. Upload Logic
            string? uploadedPath = null;
                if (!string.IsNullOrEmpty(request.Dto.ImageURL) && request.Dto.ImageURL.StartsWith("data:image"))
                {
                    var fileName = $"category_{DateTime.Now.Ticks}.png"; 
                    var uploadResult = await _imageUploadService.UploadBase64Async(request.Dto.ImageURL, fileName, "categories");
                    if (uploadResult.IsFailure)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result.Failure<CategoryDto>(uploadResult.Error);
                    }
                    uploadedPath = uploadResult.Value.Url;
                    request.Dto.ImageURL = uploadedPath;
                }

            // 2. Fetch Entity
            var entity = await _repository.GetByCodeAsync(request.Code, cancellationToken);
            if (entity == null)
            {
                 await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                 return Result.Failure<CategoryDto>(Error.NotFound(MessageConstants.Category, request.Code));
            }

            // 3. Update Entity
            _mapper.Map(request.Dto, entity);
            _repository.Update(entity);
            
            // 4. Handle Old Files (Deleting old image if new one uploaded)
            var urlsToDelete = new List<string>();
            if (!string.IsNullOrEmpty(uploadedPath))
            {
                // Find old files linked to this category
                var oldFilesToDelete = await _context.TblFiles
                    .Where(f => f.MasterCode == request.Code && f.MasterType == "Category")
                    .ToListAsync(cancellationToken);

                // Unlink or Mark for Delete
                foreach (var oldFile in oldFilesToDelete)
                {
                    if (!string.IsNullOrEmpty(oldFile.Path)) urlsToDelete.Add(oldFile.Path);
                    _context.TblFiles.Remove(oldFile);
                }
                
                // Link New File
                var newFile = await _context.TblFiles.FirstOrDefaultAsync(f => f.Path == uploadedPath, cancellationToken);
                if (newFile != null)
                {
                    newFile.MasterCode = entity.Code;
                    newFile.MasterType = "Category";
                }
                
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 5. Commit
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // 6. Post-Commit Physical Delete
            // 6. Post-Commit Physical Delete
             await _imageUploadService.DeleteImagesAsync(urlsToDelete);

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
        // Check if category has products
        var productCount = await _productRepository.CountAsync(p => p.CategoryCode == request.Code, cancellationToken);

        if (productCount > 0)
        {
            var category = await _repository.GetByCodeAsync(request.Code, cancellationToken);
            return Result.Failure(Error.Conflict(MessageConstants.Conflict,
                MessageConstants.Get(MessageConstants.CategoryHasProducts, category?.Name ?? request.Code, productCount)));
        }

        // Fetch linked files for cleanup
        var linkedFiles = await _context.TblFiles
            .Where(f => f.MasterCode == request.Code && f.MasterType == "Category")
            .ToListAsync(cancellationToken);

        var result = await DeleteAsync(request.Code, MessageConstants.Category, cancellationToken, softDelete: false);
        
        if (result.IsSuccess)
        {
             var pathsToDelete = linkedFiles
                .Where(f => !string.IsNullOrEmpty(f.Path))
                .Select(f => f.Path)
                .ToList();

             if (pathsToDelete.Any())
             {
                 await _imageUploadService.DeleteImagesAsync(pathsToDelete);
             }
        }
        
        return result;
    }

    public async Task<Result> Handle(DeleteMultipleCommand<TblCategory> request, CancellationToken cancellationToken)
    {
        // Check for active products
        // We construct the query effectively selecting the CategoryCodes that are "blocked" (in use by active products)
        var blockedCodesQuery = _productRepository.AsQueryable()
            .Where(p => request.Codes.Contains(p.CategoryCode) && p.IsActive == true)
            .Select(p => p.CategoryCode!); // Nullable check?

        var checkResult = await CheckBlockingDependenciesAsync(blockedCodesQuery, "products", cancellationToken);
        if (checkResult.IsFailure) return checkResult;

        return await DeleteMultipleAsync(request.Codes, MessageConstants.Category, cancellationToken);
    }

    public async Task<Result<CategoryDto>> Handle(GetByCodeQuery<CategoryDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<CategoryDto>(request.Code, MessageConstants.Category, cancellationToken);
    }

    public async Task<Result<CategoryStatsDto>> Handle(GetCategoryStatsQuery request, CancellationToken cancellationToken)
    {
        var total = await _repository.CountAsync(x => true, cancellationToken);
        var active = await _repository.CountAsync(x => x.IsActive, cancellationToken);
        var main = await _repository.CountAsync(x => x.ParentCode == null, cancellationToken);

        return Result.Success(new CategoryStatsDto
        {
            Total = total,
            Active = active,
            Main = main
        });
    }
}
