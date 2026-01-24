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

using Dapper;
using Newtonsoft.Json.Linq;

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
    private readonly IBaseUrlService _baseUrlService;
    private readonly IApplicationDbContext _context;

    public ProductHandlers(
        IRepository<TblProduct> repository, 
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IDapperContext dapperContext,
        IBaseUrlService baseUrlService,
        IImageUploadService imageUploadService,
        IApplicationDbContext context) 
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _baseUrlService = baseUrlService;
        _imageUploadService = imageUploadService;
        _context = context;
    }


    public async Task<Result<PagedResult<ProductDto>>> Handle(GetPagedQuery<ProductDto> request, CancellationToken cancellationToken)
    {
        // Use request.Searching directly (already contains search conditions from frontend)
        var searchFields = request.Searching ?? new List<SearchDTO>();

        // // Add IsActive filter by default
        // if (!searchFields.Any(s => s.SearchField == "IsActive"))
        // {
        //     searchFields.Add(new SearchDTO { SearchField = "IsActive", SearchCondition = SearchCondition.Equal, SearchValue = true });
        // }
        
        // // Category Filter (for GetProductsQuery)
        // string? categoryCode = null;
        // if (request is GetProductsQuery productsQuery)
        // {
        //     categoryCode = productsQuery.CategoryCode;
        // }

        // if (!string.IsNullOrWhiteSpace(categoryCode))
        // {
        //      searchFields.Add(new SearchDTO { SearchField = "CategoryCode", SearchCondition = SearchCondition.Equal, SearchValue = categoryCode });
        // }

        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };

        // Call Base Handler
        var result = await GetPagedDapperAsync<ProductDto>(request.PageIndex, request.PageSize, searchFields, sortDTO, null, request.Fields, cancellationToken);

        // 6. Populate Images Logic
        if (result.IsSuccess && result.Value.Items.Any())
        {
            using var connection = _dapperContext.CreateConnection();
            var productCodes = result.Value.Items.Select(p => p.Code).ToList();
            var fileSql = @"SELECT * FROM ""TblFile"" WHERE ""MasterCode"" = ANY(@Codes) AND ""MasterType"" = 'Product'";
            var files = await connection.QueryAsync<TblFile>(fileSql, new { Codes = productCodes });
            
            var fileGroups = files.GroupBy(f => f.MasterCode).ToDictionary(g => g.Key, g => g.ToList());
            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');

            foreach (var dto in result.Value.Items)
            {
                if (fileGroups.TryGetValue(dto.Code, out var productFiles))
                {
                    dto.ProductImages = productFiles.Select(f => new ProductImageDto
                    {
                        Code = f.Code,
                        ImageURL = f.Path.StartsWith("http") ? f.Path : $"{baseUrl}/{f.Path.TrimStart('/')}",
                        AltText = f.OriginalName,
                        IsPrimary = productFiles.IndexOf(f) == 0 
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
            
            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');
            result.Value.ProductImages = files.Select(f => new ProductImageDto
            {
                 Code = f.Code,
                 ImageURL = f.Path.StartsWith("http") ? f.Path : $"{baseUrl}/{f.Path.TrimStart('/')}",
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



            var supplierCode = string.IsNullOrWhiteSpace(dto.SupplierCode) ? null : dto.SupplierCode;
            var product = TblProduct.Create(dto.Name, dto.Price, dto.StockQuantity ?? 0, dto.CategoryCode, dto.CostPrice, 
                dto.Weight, supplierCode, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size);
            
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
                // Need to normalize URLs to relative paths (e.g. /uploads/...) to match DB
                var uniqueUrls = finalUrls.Distinct().ToList();
                var relativeUrls = uniqueUrls.Select(u => 
                {
                    if (Uri.TryCreate(u, UriKind.Absolute, out var uri))
                        return uri.AbsolutePath;
                    return u; // Already relative or invalid
                }).ToList();

                var filesToLink = await _context.TblFiles
                    .Where(f => relativeUrls.Contains(f.Path))
                    .ToListAsync(cancellationToken);
                
                foreach(var file in filesToLink) 
                {
                    file.MasterCode = product.Code;
                    file.MasterType = "Product";
                }
            }

            // product.SetAttributes...

            await _repository.AddAsync(product, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var productDto = _mapper.Map<ProductDto>(product);
            
            // Populate images for response
            var finalFiles = await _context.TblFiles
                .Where(f => f.MasterCode == product.Code && f.MasterType == "Product")
                .ToListAsync(cancellationToken);

            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');
            productDto.ProductImages = finalFiles.Select(f => new ProductImageDto
            {
                 Code = f.Code,
                 ImageURL = f.Path.StartsWith("http") ? f.Path : $"{baseUrl}/{f.Path.TrimStart('/')}",
                 AltText = f.OriginalName,
                 IsPrimary = finalFiles.IndexOf(f) == 0
            }).ToList();

            return Result.Success(productDto);
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


            var supplierCode = string.IsNullOrWhiteSpace(request.Dto.SupplierCode) ? null : request.Dto.SupplierCode;
            
            product.UpdateInfo(request.Dto.Name ?? product.Name, request.Dto.Price ?? product.Price, request.Dto.Description ?? product.Description, 
                request.Dto.CategoryCode ?? product.CategoryCode, request.Dto.CostPrice ?? product.CostPrice, request.Dto.StockQuantity ?? product.StockQuantity,
                request.Dto.Weight ?? product.Weight, supplierCode ?? product.SupplierCode, request.Dto.Color ?? product.Color, 
                request.Dto.Power ?? product.Power, request.Dto.Voltage ?? product.Voltage, request.Dto.Material ?? product.Material, 
                request.Dto.Size ?? product.Size);

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

                // Match by File Name (Robust against path variations)
                // Extract filenames from all kept URLs (both existing and newly uploaded)
                // Handles http://.../file.png, /uploads/file.png, file.png
                var keptFileNames = keptUrls
                    .Select(u => u.Split('/', '\\').Last())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // 1. Get currently linked files
                var currentFiles = await _context.TblFiles
                    .Where(f => f.MasterCode == product.Code)
                    .ToListAsync(cancellationToken);

                // 2. Identify files to remove (Current but Filename NOT in Kept)
                var filesToRemove = currentFiles
                    .Where(f => !keptFileNames.Contains(f.FileName, StringComparer.OrdinalIgnoreCase))
                    .ToList();
                
                var pathsToDelete = new List<string>();
                foreach (var f in filesToRemove)
                {
                    f.MasterCode = null; // Unlink
                    f.IsActive = false; // Soft delete
                    
                    if (!string.IsNullOrEmpty(f.Path))
                    {
                        pathsToDelete.Add(f.Path);
                    }
                }
                
                if (pathsToDelete.Any())
                {
                    await _imageUploadService.DeleteImagesAsync(pathsToDelete);
                }

                // 3. Link new files (Filename in Kept but not linked to this product)
                 var potentialFiles = await _context.TblFiles
                    .Where(f => keptFileNames.Contains(f.FileName) && f.MasterCode != product.Code)
                    .ToListAsync(cancellationToken);
                
                foreach (var f in potentialFiles)
                {
                     // Double check in memory
                     if (keptFileNames.Contains(f.FileName, StringComparer.OrdinalIgnoreCase))
                     {
                        f.MasterCode = product.Code;
                        f.MasterType = "Product";
                     }
                }
            }
            
            _repository.Update(product);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var productDto = _mapper.Map<ProductDto>(product);
            
            // Populate images for response
            var finalFiles = await _context.TblFiles
                .Where(f => f.MasterCode == product.Code && f.MasterType == "Product")
                .ToListAsync(cancellationToken);

            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');
            productDto.ProductImages = finalFiles.Select(f => new ProductImageDto
            {
                 Code = f.Code,
                 ImageURL = f.Path.StartsWith("http") ? f.Path : $"{baseUrl}/{f.Path.TrimStart('/')}",
                 AltText = f.OriginalName,
                 IsPrimary = finalFiles.IndexOf(f) == 0
            }).ToList();

            return Result.Success(productDto);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result> Handle(DeleteCommand<TblProduct> request, CancellationToken cancellationToken)
    {
        // 1. Fetch images linked to this product (Active ones)
        var linkedFiles = await _context.TblFiles
            .Where(f => f.MasterCode == request.Code && f.MasterType == "Product" && f.IsActive)
            .ToListAsync(cancellationToken);

        // 2. Delete Product (Soft Delete)
        var result = await DeleteAsync(request.Code, MessageConstants.Product, cancellationToken);

        if (result.IsSuccess)
        {
            // 3. Strict Cleanup: Delete images from Cloudinary and Soft Delete in DB (Bulk)
            var pathsToDelete = new List<string>();
            foreach (var f in linkedFiles)
            {
                f.IsActive = false;
                if (!string.IsNullOrEmpty(f.Path))
                {
                     pathsToDelete.Add(f.Path);
                }
            }
            
            if (pathsToDelete.Any())
            {
                 try 
                 {
                     await _imageUploadService.DeleteImagesAsync(pathsToDelete);
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"Failed to bulk delete images: {ex.Message}");
                 }
            }
            
            await _context.SaveChangesAsync(cancellationToken);
        }

        return result;
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
                    var supplierCode = string.IsNullOrWhiteSpace(dto.SupplierCode) ? null : dto.SupplierCode;
                    product.UpdateFromImport(dto.Name, dto.Price, dto.StockQuantity, dto.CategoryCode, dto.Description, dto.IsActive, dto.Weight, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size, supplierCode);
                    _repository.Update(product);
                }
                else
                {
                    var supplierCode = string.IsNullOrWhiteSpace(dto.SupplierCode) ? null : dto.SupplierCode;
                    product = TblProduct.Create(dto.Name, dto.Price, dto.StockQuantity ?? 0, dto.CategoryCode, null, dto.Weight, supplierCode, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size);
                    product.UpdateFromImport(dto.Name, dto.Price, dto.StockQuantity, dto.CategoryCode, dto.Description, dto.IsActive, dto.Weight, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size, supplierCode);
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
