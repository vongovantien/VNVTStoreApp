using MediatR;
using AutoMapper;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs.Import;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Brands.Commands;

public record ImportBrandsCommand(Stream FileStream) : IRequest<Result<int>>;

public class ImportBrandsHandler : BaseHandler<TblBrand>, IRequestHandler<ImportBrandsCommand, Result<int>>
{
    public ImportBrandsHandler(
        IRepository<TblBrand> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<int>> Handle(ImportBrandsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var rows = ExcelImportHelper.Import<BrandImportDto>(request.FileStream);
            var importedCount = 0;

            foreach (var dto in rows)
            {
                if (string.IsNullOrEmpty(dto.Name)) continue;

                TblBrand? brand = null;

                if (!string.IsNullOrEmpty(dto.Code))
                {
                    brand = await _repository.GetByCodeAsync(dto.Code, cancellationToken);
                }

                if (brand != null)
                {
                    brand.Name = dto.Name;
                    brand.Description = dto.Description;
                    brand.IsActive = dto.IsActive ?? true;
                    brand.UpdatedAt = DateTime.Now;
                    _repository.Update(brand);
                }
                else
                {
                    brand = new TblBrand
                    {
                        Name = dto.Name,
                        Description = dto.Description,
                        IsActive = dto.IsActive ?? true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                        // Code generation logic
                    };
                    await _repository.AddAsync(brand, cancellationToken);
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
