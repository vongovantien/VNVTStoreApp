using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Products.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Products.Handlers;

public class GetProductStatsHandler : BaseHandler<TblProduct>,
    IRequestHandler<GetProductStatsQuery, Result<ProductStatsDto>>
{
    public GetProductStatsHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<ProductStatsDto>> Handle(GetProductStatsQuery request, CancellationToken cancellationToken)
    {
        var total = await _repository.CountAsync(p => p.ModifiedType != ModificationType.Delete.ToString(), cancellationToken);
        var lowStock = await _repository.CountAsync(p => p.ModifiedType != ModificationType.Delete.ToString() && p.StockQuantity > 0 && p.StockQuantity <= (p.MinStockLevel ?? 10), cancellationToken);
        var outOfStock = await _repository.CountAsync(p => p.ModifiedType != ModificationType.Delete.ToString() && (p.StockQuantity == null || p.StockQuantity <= 0), cancellationToken);

        return Result.Success(new ProductStatsDto
        {
            Total = total,
            LowStock = lowStock,
            OutOfStock = outOfStock
        });
    }
}
