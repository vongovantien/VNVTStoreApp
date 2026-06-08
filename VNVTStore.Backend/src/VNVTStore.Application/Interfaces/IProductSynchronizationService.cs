using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Interfaces;

public interface IProductSynchronizationService
{
    Task SyncDetailsAsync(TblProduct product, List<CreateProductDetailDto>? details, CancellationToken cancellationToken);
    Task SyncUnitsAsync(TblProduct product, List<CreateUnitDto>? units, CancellationToken cancellationToken);
    Task SyncVariantsAsync(TblProduct product, List<CreateProductVariantDto>? variants, CancellationToken cancellationToken);
}
