using MediatR;
using Microsoft.EntityFrameworkCore;

using VNVTStore.Application.Common;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.DTOs.Import;
using VNVTStore.Application.Promotions.Commands;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Application.Promotions.Handlers;

public class ImportPromotionsHandler : IRequestHandler<ImportPromotionsCommand, Result<int>>
{
    private readonly IRepository<TblPromotion> _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImportPromotionsHandler(IRepository<TblPromotion> promotionRepository, IUnitOfWork unitOfWork)
    {
        _promotionRepository = promotionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(ImportPromotionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var rows = ExcelImportHelper.Import<PromotionImportDto>(request.FileStream);
            var importedCount = 0;

            foreach (var dto in rows)
            {
                if (string.IsNullOrEmpty(dto.Code)) continue;

                var existing = await _promotionRepository.GetByCodeAsync(dto.Code, cancellationToken);
                if (existing != null)
                {
                    // Update
                    existing.Name = dto.Name;
                    existing.Description = dto.Description;
                    existing.DiscountType = dto.DiscountType.ToUpper();
                    existing.DiscountValue = dto.DiscountValue;
                    existing.MinOrderAmount = dto.MinOrderAmount;
                    existing.MaxDiscountAmount = dto.MaxDiscountAmount;
                    existing.StartDate = dto.StartDate ?? DateTime.Today;
                    existing.EndDate = dto.EndDate ?? DateTime.Today.AddDays(7);
                    existing.UsageLimit = dto.UsageLimit;
                    existing.IsActive = dto.IsActive ?? true;
                    _promotionRepository.Update(existing);
                }
                else
                {
                    // Create
                    var newPromo = new TblPromotion
                    {
                        Code = dto.Code,
                        Name = dto.Name,
                        Description = dto.Description,
                        DiscountType = dto.DiscountType.ToUpper(),
                        DiscountValue = dto.DiscountValue,
                        MinOrderAmount = dto.MinOrderAmount,
                        MaxDiscountAmount = dto.MaxDiscountAmount,
                        StartDate = dto.StartDate ?? DateTime.Today,
                        EndDate = dto.EndDate ?? DateTime.Today.AddDays(7),
                        UsageLimit = dto.UsageLimit,
                        IsActive = dto.IsActive ?? true
                    };
                    await _promotionRepository.AddAsync(newPromo, cancellationToken);
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
