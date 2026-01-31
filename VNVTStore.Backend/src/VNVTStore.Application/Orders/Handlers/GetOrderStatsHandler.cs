using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Orders.Handlers;

public class GetOrderStatsHandler : BaseHandler<TblOrder>, 
    IRequestHandler<GetOrderStatsQuery, Result<OrderStatsDto>>
{
    public GetOrderStatsHandler(
        IRepository<TblOrder> orderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(orderRepository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<OrderStatsDto>> Handle(GetOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = new OrderStatsDto
        {
            Total = await _repository.CountAsync(null, cancellationToken),
            Pending = await _repository.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken),
            Shipping = await _repository.CountAsync(o => o.Status == OrderStatus.Shipped, cancellationToken),
            Delivered = await _repository.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken),
            Cancelled = await _repository.CountAsync(o => o.Status == OrderStatus.Cancelled, cancellationToken)
        };

        return Result.Success(stats);
    }
}
