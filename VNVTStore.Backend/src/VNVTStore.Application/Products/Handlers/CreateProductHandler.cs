using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Products.Handlers;

public class CreateProductHandler : BaseHandler<TblProduct>,
    IRequestHandler<CreateCommand<CreateProductDto, ProductDto>, Result<ProductDto>>
{
    private readonly IFileService _fileService;
    private readonly IBaseUrlService _baseUrlService;
    private readonly IApplicationDbContext _context;

    public CreateProductHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IBaseUrlService baseUrlService,
        IFileService fileService,
        IApplicationDbContext context)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _baseUrlService = baseUrlService;
        _fileService = fileService;
        _context = context;
    }

    public async Task<Result<ProductDto>> Handle(CreateCommand<CreateProductDto, ProductDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var dto = request.Dto;

            var supplierCode = string.IsNullOrWhiteSpace(dto.SupplierCode) ? null : dto.SupplierCode;
            var product = TblProduct.Create(dto.Name, dto.Price, dto.WholesalePrice, dto.StockQuantity ?? 0, dto.CategoryCode, dto.CostPrice, 
                supplierCode, dto.BrandCode, dto.BaseUnit, dto.IsNew, dto.IsFeatured);

            if (dto.Details != null && dto.Details.Any())
            {
                foreach (var detail in dto.Details)
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
            
            if (dto.Images != null && dto.Images.Any())
            {
                var saveImagesResult = await _fileService.SaveAndLinkImagesAsync(
                    product.Code, 
                    "TblProduct", 
                    dto.Images, 
                    "products", 
                    cancellationToken);
                
                if (saveImagesResult.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<ProductDto>(saveImagesResult.Error);
                }
            }

            if (dto.ProductUnits != null && dto.ProductUnits.Any())
            {
                foreach (var unitDto in dto.ProductUnits)
                {
                    var unitCatalog = await _context.TblUnits
                        .FirstOrDefaultAsync(u => u.Name == unitDto.UnitName, cancellationToken);
                    
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

            if (dto.Variants != null && dto.Variants.Any())
            {
                foreach (var variantDto in dto.Variants)
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

            await _repository.AddAsync(product, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var productDto = _mapper.Map<ProductDto>(product);
            
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
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private bool IsBase64String(string s) => s.StartsWith("data:image");
}
