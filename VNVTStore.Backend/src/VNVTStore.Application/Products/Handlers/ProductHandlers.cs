using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Products.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.DTOs.Import;
using VNVTStore.Application.Products.Commands;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Products.Handlers;

public class ProductHandlers : BaseHandler<TblProduct>,
    IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>,
    IRequestHandler<GetPagedQuery<ProductDto>, Result<PagedResult<ProductDto>>>,
    IRequestHandler<GetByCodeQuery<ProductDto>, Result<ProductDto>>,
    IRequestHandler<CreateCommand<CreateProductDto, ProductDto>, Result<ProductDto>>,
    IRequestHandler<UpdateCommand<UpdateProductDto, ProductDto>, Result<ProductDto>>,
    IRequestHandler<DeleteCommand<TblProduct>, Result>,
    IRequestHandler<ImportProductsCommand, Result<int>>
{
    private readonly IImageUploadService _imageUploadService;
    private readonly IApplicationDbContext _context;

    public ProductHandlers(IRepository<TblProduct> repository, IUnitOfWork unitOfWork, IMapper mapper, IImageUploadService imageUploadService, IApplicationDbContext context) 
        : base(repository, unitOfWork, mapper)
    {
        _imageUploadService = imageUploadService;
        _context = context;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetPagedQuery<ProductDto> request, CancellationToken cancellationToken)
    {
        string? categoryCode = null;
        if (request is GetProductsQuery productsQuery)
        {
            categoryCode = productsQuery.CategoryCode;
        }

        var result = await GetPagedAsync<ProductDto>(
            request.PageIndex,
            request.PageSize,
            cancellationToken,
            predicate: p => (p.IsActive == true) && (p.ModifiedType != ModificationType.Delete.ToString()) &&
                           (string.IsNullOrWhiteSpace(request.Search) || p.Name.Contains(request.Search) || p.Code.Contains(request.Search) || (p.Sku != null && p.Sku.Contains(request.Search))) &&
                           (string.IsNullOrWhiteSpace(categoryCode) || p.CategoryCode == categoryCode),
            includes: q => QueryHelper.ApplyFilters(
                q.Include(p => p.CategoryCodeNavigation), 
                request.Filters),
            orderBy: q => {
                if (request.SortDTO != null && !string.IsNullOrWhiteSpace(request.SortDTO.SortBy))
                {
                    var sortBy = request.SortDTO.SortBy.ToLower();
                    if (sortBy == nameof(TblProduct.Price).ToLower())
                        return request.SortDTO.SortDescending ? q.OrderByDescending(p => p.Price) : q.OrderBy(p => p.Price);
                    if (sortBy == nameof(TblProduct.Name).ToLower())
                        return request.SortDTO.SortDescending ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name);
                    if (sortBy == nameof(TblProduct.CreatedAt).ToLower())
                        return request.SortDTO.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt);
                    
                    return q.OrderByDescending(p => p.CreatedAt);
                }
                return q.OrderByDescending(p => p.CreatedAt);
            },
            fields: request.Fields);

        if (result.IsSuccess && result.Value.Items.Any())
        {
            var productCodes = result.Value.Items.Select(p => p.Code).ToList();
            var files = await _context.TblFiles
                .Where(f => productCodes.Contains(f.MasterCode) && f.MasterType == "Product")
                .ToListAsync(cancellationToken);
            
            var fileGroups = files.GroupBy(f => f.MasterCode).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var dto in result.Value.Items)
            {
                if (fileGroups.TryGetValue(dto.Code, out var productFiles))
                {
                    dto.ProductImages = productFiles.Select(f => new ProductImageDto
                    {
                        Code = f.Code,
                        ImageUrl = f.Path,
                        AltText = f.OriginalName,
                        IsPrimary = productFiles.IndexOf(f) == 0 // Assumption
                    }).ToList();
                }
            }
        }

        return result;
    }
    
    // Explicit handler for GetProductsQuery - delegates to the generic handler
    public Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return Handle((GetPagedQuery<ProductDto>)request, cancellationToken);
    }

    public async Task<Result<ProductDto>> Handle(GetByCodeQuery<ProductDto> request, CancellationToken cancellationToken)
    {
        var result = await GetByCodeIncludeChildrenAsync<ProductDto>(
            request.Code, 
            MessageConstants.Product, 
            cancellationToken,
            includes: q => q.Include(p => p.CategoryCodeNavigation));

        if (result.IsSuccess)
        {
            var files = await _context.TblFiles
                .Where(f => f.MasterCode == request.Code && f.MasterType == "Product")
                .ToListAsync(cancellationToken);
            
            result.Value.ProductImages = files.Select(f => new ProductImageDto
            {
                 Code = f.Code,
                 ImageUrl = f.Path,
                 AltText = f.OriginalName,
                 IsPrimary = files.IndexOf(f) == 0
            }).ToList();
        }

        return result;
    }

    public async Task<Result<ProductDto>> Handle(CreateCommand<CreateProductDto, ProductDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var dto = request.Dto;

            if (!string.IsNullOrWhiteSpace(dto.Sku))
            {
                var existingSku = await _repository.FindAsync(p => p.Sku == dto.Sku && p.ModifiedType != ModificationType.Delete.ToString(), cancellationToken);
                if (existingSku != null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<ProductDto>(Error.Conflict(MessageConstants.AlreadyExists, "SKU", dto.Sku));
                }
            }

            var sku = string.IsNullOrWhiteSpace(dto.Sku) 
                ? $"SKU{DateTime.Now.Ticks.ToString().Substring(10)}" 
                : dto.Sku;

            var product = TblProduct.Create(dto.Name, dto.Price, dto.StockQuantity ?? 0, dto.CategoryCode, sku, dto.CostPrice, 
                dto.Weight, dto.SupplierCode, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size);
            
            if (dto.Images != null && dto.Images.Any())
            {
                var base64Images = new List<(string Content, string FileName)>();
                var finalUrls = new List<string>();

                foreach (var imgStr in dto.Images)
                {
                    if (IsBase64String(imgStr))
                    {
                        string extension = ".png"; 
                        if (imgStr.StartsWith("data:image/"))
                        {
                            try {
                                var mime = imgStr.Substring(5, imgStr.IndexOf(";") - 5);
                                extension = "." + mime.Split('/')[1];
                            } catch {}
                        }
                        base64Images.Add((imgStr, $"product_{Guid.NewGuid()}{extension}"));
                    }
                    else
                    {
                        finalUrls.Add(imgStr);
                    }
                }

                if (base64Images.Any())
                {
                    var uploadResult = await _imageUploadService.UploadBase64ImagesAsync(base64Images, "products");
                    if (uploadResult.IsFailure)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result.Failure<ProductDto>(uploadResult.Error);
                    }
                    finalUrls.AddRange(uploadResult.Value.Select(f => f.Url));
                }

                // Link files to product
                var uniqueUrls = finalUrls.Distinct().ToList();
                var filesToLink = await _context.TblFiles
                    .Where(f => uniqueUrls.Contains(f.Path))
                    .ToListAsync(cancellationToken);
                
                foreach(var file in filesToLink) 
                {
                    file.MasterCode = product.Code;
                    file.MasterType = "Product";
                    // TblFile Save is handled by Context, but we need to mark them modified? 
                    // They are tracked by _context (if shared context).
                    // _context is IApplicationDbContext.
                }
            }

            // product.SetAttributes...

            await _repository.AddAsync(product, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<ProductDto>(product));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<ProductDto>> Handle(UpdateCommand<UpdateProductDto, ProductDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await _repository.GetByCodeAsync(request.Code, cancellationToken);
                
            if (product == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<ProductDto>(Error.NotFound(MessageConstants.Product, request.Code));
            }

            // SKU Check Code...
            if (!string.IsNullOrWhiteSpace(request.Dto.Sku) && request.Dto.Sku != product.Sku)
            {
                var existingSku = await _repository.FindAsync(p => p.Sku == request.Dto.Sku && p.Code != request.Code && p.ModifiedType != ModificationType.Delete.ToString(), cancellationToken);
                if (existingSku != null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<ProductDto>(Error.Conflict(MessageConstants.AlreadyExists, "SKU", request.Dto.Sku));
                }
            }

            product.UpdateInfo(request.Dto.Name ?? product.Name, request.Dto.Price ?? product.Price, request.Dto.Description ?? product.Description, 
                request.Dto.CategoryCode ?? product.CategoryCode, request.Dto.CostPrice ?? product.CostPrice, request.Dto.StockQuantity ?? product.StockQuantity,
                request.Dto.Weight ?? product.Weight, request.Dto.SupplierCode ?? product.SupplierCode, request.Dto.Color ?? product.Color, 
                request.Dto.Power ?? product.Power, request.Dto.Voltage ?? product.Voltage, request.Dto.Material ?? product.Material, 
                request.Dto.Size ?? product.Size, request.Dto.Sku ?? product.Sku);

            if (request.Dto.Images != null)
            {
                var base64Images = new List<(string Content, string FileName)>();
                var keptUrls = new List<string>();

                foreach (var imgStr in request.Dto.Images)
                {
                    if (IsBase64String(imgStr))
                    {
                         string extension = ".png";
                         if (imgStr.StartsWith("data:image/"))
                         {
                             try {
                                 var mime = imgStr.Substring(5, imgStr.IndexOf(";") - 5);
                                 extension = "." + mime.Split('/')[1];
                             } catch {}
                         }
                         base64Images.Add((imgStr, $"product_upd_{Guid.NewGuid()}{extension}"));
                    }
                    else
                    {
                        keptUrls.Add(imgStr);
                    }
                }

                if (base64Images.Any())
                {
                    var uploadResult = await _imageUploadService.UploadBase64ImagesAsync(base64Images, "products");
                    if (uploadResult.IsFailure)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result.Failure<ProductDto>(uploadResult.Error);
                    }
                    keptUrls.AddRange(uploadResult.Value.Select(f => f.Url));
                }

                // 1. Get currently linked files
                var currentFiles = await _context.TblFiles
                    .Where(f => f.MasterCode == product.Code)
                    .ToListAsync(cancellationToken);

                // 2. Identify files to remove (Current but not in Kept)
                var uniqueKeptUrls = keptUrls.Distinct().ToList();
                var filesToRemove = currentFiles.Where(f => !uniqueKeptUrls.Contains(f.Path)).ToList();
                
                foreach (var f in filesToRemove)
                {
                    f.MasterCode = null; // Unlink
                    f.IsActive = false; // Soft delete
                    // Optionally delete file from disk: _imageUploadService.DeleteImageAsync(f.Path);
                }

                // 3. Link new files (in Kept but not linked)
                // We need to fetch the orphans (newly uploaded)
                 var filesToLink = await _context.TblFiles
                    .Where(f => uniqueKeptUrls.Contains(f.Path) && f.MasterCode != product.Code)
                    .ToListAsync(cancellationToken);

                foreach (var f in filesToLink)
                {
                    f.MasterCode = product.Code;
                    f.MasterType = "Product";
                }
            }
            
            _repository.Update(product);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<ProductDto>(product));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result> Handle(DeleteCommand<TblProduct> request, CancellationToken cancellationToken)
    {
        return await DeleteAsync(request.Code, MessageConstants.Product, cancellationToken);
    }

    private bool IsBase64String(string s)
    {
        return s.StartsWith("data:image");
    }

    public async Task<Result<int>> Handle(ImportProductsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var rows = ExcelImportHelper.Import<ProductImportDto>(request.FileStream);
            var importedCount = 0;

            foreach (var dto in rows)
            {
                if (string.IsNullOrEmpty(dto.Name)) dto.Name = "Unnamed Product"; 

                TblProduct? product = null;

                if (!string.IsNullOrEmpty(dto.Code))
                {
                    product = await _repository.GetByCodeAsync(dto.Code, cancellationToken);
                }

                if (product != null)
                {
                    product.UpdateFromImport(dto.Name, dto.Price, dto.StockQuantity, dto.CategoryCode, dto.Sku, dto.Description, dto.IsActive, dto.Weight, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size, dto.SupplierCode);
                    _repository.Update(product);
                }
                else
                {
                    product = TblProduct.Create(dto.Name, dto.Price, dto.StockQuantity ?? 0, dto.CategoryCode, dto.Sku, null, dto.Weight, dto.SupplierCode, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size);
                    product.UpdateFromImport(dto.Name, dto.Price, dto.StockQuantity, dto.CategoryCode, dto.Sku, dto.Description, dto.IsActive, dto.Weight, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size, dto.SupplierCode);
                    await _repository.AddAsync(product, cancellationToken);
                }
                importedCount++;
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(importedCount);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>("ImportError", ex.Message);
        }
    }
}
