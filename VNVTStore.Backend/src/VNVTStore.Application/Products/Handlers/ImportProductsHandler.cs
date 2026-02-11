using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.DTOs.Import;
using VNVTStore.Application.Products.Commands;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Products.Handlers;

public class ImportProductsHandler : BaseHandler<TblProduct>,
    IRequestHandler<ImportProductsCommand, Result<int>>
{
    public ImportProductsHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
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
                    product.UpdateFromImport(dto.Name, dto.Price, dto.WholesalePrice, dto.StockQuantity, dto.CategoryCode, dto.Description, dto.IsActive, supplierCode, dto.BrandCode);
                    _repository.Update(product);
                }
                else
                {
                    var supplierCode = string.IsNullOrWhiteSpace(dto.SupplierCode) ? null : dto.SupplierCode;
                    product = TblProduct.Create(dto.Name, dto.Price, dto.WholesalePrice, dto.StockQuantity ?? 0, dto.CategoryCode, null, supplierCode, dto.BrandCode, dto.BaseUnit);
                    product.UpdateFromImport(dto.Name, dto.Price, dto.WholesalePrice, dto.StockQuantity, dto.CategoryCode, dto.Description, dto.IsActive, supplierCode, dto.BrandCode);
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
