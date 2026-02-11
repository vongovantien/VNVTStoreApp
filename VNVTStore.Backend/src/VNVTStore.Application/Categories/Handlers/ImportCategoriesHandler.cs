using MediatR;
using AutoMapper;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs.Import;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Categories.Commands;

public record ImportCategoriesCommand(Stream FileStream) : IRequest<Result<int>>;

public class ImportCategoriesHandler : BaseHandler<TblCategory>, IRequestHandler<ImportCategoriesCommand, Result<int>>
{
    public ImportCategoriesHandler(
        IRepository<TblCategory> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<int>> Handle(ImportCategoriesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var rows = ExcelImportHelper.Import<CategoryImportDto>(request.FileStream);
            var importedCount = 0;

            foreach (var dto in rows)
            {
                if (string.IsNullOrEmpty(dto.Name)) continue; 

                TblCategory? category = null;

                if (!string.IsNullOrEmpty(dto.Code))
                {
                    category = await _repository.GetByCodeAsync(dto.Code, cancellationToken);
                }

                if (category != null)
                {
                    category.Name = dto.Name;
                    category.Description = dto.Description;
                    category.IsActive = dto.IsActive ?? true;
                    category.ParentCode = string.IsNullOrWhiteSpace(dto.ParentCategoryCode) ? null : dto.ParentCategoryCode;
                    category.UpdatedAt = DateTime.Now;
                    _repository.Update(category);
                }
                else
                {
                    // Create logic - basic
                    category = new TblCategory 
                    {
                        Name = dto.Name,
                        Description = dto.Description,
                        IsActive = dto.IsActive ?? true,
                        ParentCode = string.IsNullOrWhiteSpace(dto.ParentCategoryCode) ? null : dto.ParentCategoryCode,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                        // Code generation should be handled by repository or EF Core if not provided
                    };
                    await _repository.AddAsync(category, cancellationToken);
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
