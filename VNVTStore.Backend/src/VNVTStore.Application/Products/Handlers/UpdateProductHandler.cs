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
    private readonly IProductSynchronizationService _syncService;

    public UpdateProductHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IBaseUrlService baseUrlService,
        IFileService fileService,
        IApplicationDbContext context,
        ILogger<UpdateProductHandler> logger,
        IProductSynchronizationService syncService)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _baseUrlService = baseUrlService;
        _fileService = fileService;
        _context = context;
        _logger = logger;
        _syncService = syncService;
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

            // Cleanly delegate synchronization to dedicated service (FOLLOWS SOLID SRP)
            await _syncService.SyncDetailsAsync(product, request.Dto.Details, cancellationToken);
            await _syncService.SyncUnitsAsync(product, request.Dto.ProductUnits, cancellationToken);
            await _syncService.SyncVariantsAsync(product, request.Dto.Variants, cancellationToken);

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
