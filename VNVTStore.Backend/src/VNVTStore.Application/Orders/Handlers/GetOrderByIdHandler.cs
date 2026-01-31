using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Orders.Handlers;

public class GetOrderByIdHandler : BaseHandler<TblOrder>, 
    IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public GetOrderByIdHandler(
        IRepository<TblOrder> orderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(orderRepository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.AsQueryable()
            .Include(o => o.TblOrderItems)
            .ThenInclude(oi => oi.ProductCodeNavigation)
            .FirstOrDefaultAsync(o => o.Code == request.orderCode, cancellationToken);
            
        if (order == null)
            return Result.Failure<OrderDto>(Error.NotFound(MessageConstants.Order, request.orderCode));

        return Result.Success(_mapper.Map<OrderDto>(order));
    }
}
