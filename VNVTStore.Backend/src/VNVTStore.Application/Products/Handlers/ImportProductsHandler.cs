using MediatR;
using Microsoft.EntityFrameworkCore;

using VNVTStore.Application.Common;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.DTOs.Import;
using VNVTStore.Application.Products.Commands;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Application.Products.Handlers;

public class ImportProductsHandler : IRequestHandler<ImportProductsCommand, Result<int>>
{
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImportProductsHandler(IRepository<TblProduct> productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
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
                    product = await _productRepository.AsQueryable().FirstOrDefaultAsync(p => p.Code == dto.Code, cancellationToken);
                }

                if (product != null)
                {
                    product.UpdateFromImport(dto.Name, dto.Price, dto.StockQuantity, dto.CategoryCode, dto.Sku, dto.Description, dto.IsActive, dto.Weight, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size, dto.SupplierCode);
                    _productRepository.Update(product);
                }
                else
                {
                    product = TblProduct.Create(dto.Name, dto.Price, dto.StockQuantity ?? 0, dto.CategoryCode, dto.Sku);
                    product.UpdateFromImport(dto.Name, dto.Price, dto.StockQuantity, dto.CategoryCode, dto.Sku, dto.Description, dto.IsActive, dto.Weight, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size, dto.SupplierCode);
                    await _productRepository.AddAsync(product, cancellationToken);
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
