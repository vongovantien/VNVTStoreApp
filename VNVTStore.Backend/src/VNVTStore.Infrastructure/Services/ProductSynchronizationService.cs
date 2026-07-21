using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Common;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Infrastructure.Services;

public class ProductSynchronizationService : IProductSynchronizationService
{
    private readonly IApplicationDbContext _context;

    public ProductSynchronizationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SyncDetailsAsync(TblProduct product, List<CreateProductDetailDto>? details, CancellationToken cancellationToken)
    {
        if (details == null) return;

        if (product.TblProductDetails.Any())
        {
            _context.TblProductDetails.RemoveRange(product.TblProductDetails);
        }
        
        product.TblProductDetails.Clear();
        
        foreach (var detail in details)
        {
            product.TblProductDetails.Add(new TblProductDetail {
                Code = CodeGenerator.New(),
                ProductCode = product.Code,
                DetailType = detail.DetailType,
                SpecName = detail.SpecName,
                SpecValue = detail.SpecValue,
                IsActive = true
            });
        }
    }

    public async Task SyncUnitsAsync(TblProduct product, List<CreateUnitDto>? units, CancellationToken cancellationToken)
    {
        if (units == null) return;

        var existingUnits = product.TblProductUnits.ToList();
        if (existingUnits.Any())
        {
            _context.TblProductUnits.RemoveRange(existingUnits);
        }
        
        product.TblProductUnits.Clear();

        foreach (var unitDto in units)
        {
            var unitCatalog = _context.TblUnits.Local.FirstOrDefault(u => u.Name == unitDto.UnitName)
                              ?? await _context.TblUnits.FirstOrDefaultAsync(u => u.Name == unitDto.UnitName, cancellationToken);
            
            if (unitCatalog == null)
            {
                unitCatalog = new TblUnit { 
                    Code = CodeGenerator.New(),
                    Name = unitDto.UnitName, 
                    IsActive = true 
                };
                await _context.TblUnits.AddAsync(unitCatalog, cancellationToken);
            }

            var newProductUnit = new TblProductUnit
            {
                Code = CodeGenerator.New(),
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

    public async Task SyncVariantsAsync(TblProduct product, List<CreateProductVariantDto>? variants, CancellationToken cancellationToken)
    {
        if (variants == null) return;

        if (product.TblProductVariants.Any())
        {
            _context.TblProductVariants.RemoveRange(product.TblProductVariants);
        }
        product.TblProductVariants.Clear();

        foreach (var variantDto in variants)
        {
            product.TblProductVariants.Add(new TblProductVariant
            {
                Code = CodeGenerator.New(),
                ProductCode = product.Code,
                SKU = variantDto.SKU,
                Attributes = variantDto.Attributes,
                Price = variantDto.Price,
                StockQuantity = variantDto.StockQuantity,
                IsActive = true
            });
        }
    }
}
