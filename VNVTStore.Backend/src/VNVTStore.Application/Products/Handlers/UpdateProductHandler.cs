using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace VNVTStore.Application.Products.Handlers;

public class UpdateProductHandler : BaseHandler<TblProduct>,
    IRequestHandler<UpdateCommand<UpdateProductDto, ProductDto>, Result<ProductDto>>
{
    private readonly IFileService _fileService;
    private readonly IBaseUrlService _baseUrlService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IBaseUrlService baseUrlService,
        IFileService fileService,
        IApplicationDbContext context,
        ILogger<UpdateProductHandler> logger)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _baseUrlService = baseUrlService;
        _fileService = fileService;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(UpdateCommand<UpdateProductDto, ProductDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await _repository.AsQueryable()
                .Include(p => p.TblProductDetails)
                .Include(p => p.TblProductUnits).ThenInclude(pu => pu.Unit)
                .Include(p => p.TblProductVariants)
                .FirstOrDefaultAsync(p => p.Code == request.Code, cancellationToken);
                
            if (product == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<ProductDto>(Error.NotFound(MessageConstants.Product, request.Code));
            }

            var supplierCode = string.IsNullOrWhiteSpace(request.Dto.SupplierCode) ? null : request.Dto.SupplierCode;
            
            product.UpdateInfo(
                request.Dto.Name ?? product.Name, 
                request.Dto.Price ?? product.Price, 
                request.Dto.WholesalePrice ?? product.WholesalePrice,
                request.Dto.Description ?? product.Description, 
                request.Dto.CategoryCode ?? product.CategoryCode, 
                request.Dto.CostPrice ?? product.CostPrice, 
                request.Dto.StockQuantity ?? product.StockQuantity,
                supplierCode ?? product.SupplierCode, 
                request.Dto.BrandCode ?? product.BrandCode, 
                request.Dto.BaseUnit ?? product.BaseUnit,
                request.Dto.MinStockLevel ?? product.MinStockLevel,
                request.Dto.BinLocation ?? product.BinLocation,
                request.Dto.VatRate ?? product.VatRate,
                request.Dto.CountryOfOrigin ?? product.CountryOfOrigin,
                request.Dto.IsNew,
                request.Dto.IsFeatured
            );

            if (request.Dto.IsActive.HasValue)
            {
                product.IsActive = request.Dto.IsActive.Value;
            }

            if (request.Dto.Details != null)
            {
                if (product.TblProductDetails.Any())
                {
                    _context.TblProductDetails.RemoveRange(product.TblProductDetails);
                }
                
                product.TblProductDetails.Clear();
                
                foreach (var detail in request.Dto.Details)
                {
                    product.TblProductDetails.Add(new TblProductDetail {
                        Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                        ProductCode = product.Code,
                        DetailType = detail.DetailType,
                        SpecName = detail.SpecName,
                        SpecValue = detail.SpecValue,
                        IsActive = true
                    });
                }
            }

            if (request.Dto.ProductUnits != null)
            {
                var existingUnits = product.TblProductUnits.ToList();
                if (existingUnits.Any())
                {
                    _context.TblProductUnits.RemoveRange(existingUnits);
                }
                
                product.TblProductUnits.Clear();

                foreach (var unitDto in request.Dto.ProductUnits)
                {
                    var unitCatalog = _context.TblUnits.Local.FirstOrDefault(u => u.Name == unitDto.UnitName)
                                      ?? await _context.TblUnits.FirstOrDefaultAsync(u => u.Name == unitDto.UnitName, cancellationToken);
                    
                    if (unitCatalog == null)
                    {
                        unitCatalog = new TblUnit { 
                            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                            Name = unitDto.UnitName, 
                            IsActive = true 
                        };
                        await _context.TblUnits.AddAsync(unitCatalog, cancellationToken);
                    }

                    var newProductUnit = new TblProductUnit
                    {
                        Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                        ProductCode = product.Code,
                        UnitCode = unitCatalog.Code, 
                        ConversionRate = unitDto.ConversionRate,
                        Price = unitDto.Price,
                        IsBaseUnit = unitDto.IsBaseUnit,
                        IsActive = true,
                        Unit = unitCatalog
                    };

                    product.TblProductUnits.Add(newProductUnit);
                }
            }

            if (request.Dto.Variants != null)
            {
                if (product.TblProductVariants.Any())
                {
                    _context.TblProductVariants.RemoveRange(product.TblProductVariants);
                }
                product.TblProductVariants.Clear();

                foreach (var variantDto in request.Dto.Variants)
                {
                    product.TblProductVariants.Add(new TblProductVariant
                    {
                        Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                        ProductCode = product.Code,
                        SKU = variantDto.SKU,
                        Attributes = variantDto.Attributes,
                        Price = variantDto.Price,
                        StockQuantity = variantDto.StockQuantity,
                        IsActive = true
                    });
                }
            }

            if (request.Dto.Images != null && request.Dto.Images.Count > 0)
            {
                var syncResult = await _fileService.SyncProductImagesAsync(
                    product.Code, 
                    request.Dto.Images, 
                    cancellationToken);
                
                if (syncResult.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<ProductDto>(syncResult.Error);
                }
            }
            
            _repository.Update(product);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var freshProduct = await _context.TblProducts
                .Include(p => p.TblProductUnits).ThenInclude(u => u.Unit)
                .Include(p => p.TblProductDetails)
                .Include(p => p.TblProductVariants)
                .Include(p => p.CategoryCodeNavigation)
                .Include(p => p.Brand)
                .Include(p => p.SupplierCodeNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == product.Code, cancellationToken);
            
            var productToMap = freshProduct ?? product;
            var productDto = _mapper.Map<ProductDto>(productToMap);
            
            var finalFiles = await _context.TblFiles
                .Where(f => f.MasterCode == product.Code && f.MasterType == "TblProduct")
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateProductHandler] Error updating product {Code}", request.Code);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private bool IsBase64String(string s) => s.StartsWith("data:image");
}
