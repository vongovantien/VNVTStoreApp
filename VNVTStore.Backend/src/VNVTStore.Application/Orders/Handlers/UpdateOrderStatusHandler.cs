using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Orders.Handlers;

public class UpdateOrderStatusHandler : BaseHandler<TblOrder>, 
    IRequestHandler<UpdateOrderStatusCommand, Result<OrderDto>>
{
    public UpdateOrderStatusHandler(
        IRepository<TblOrder> orderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(orderRepository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByCodeAsync(request.orderCode, cancellationToken);
        if (order == null)
            return Result.Failure<OrderDto>(Error.NotFound(MessageConstants.Order, request.orderCode));

        order.UpdateStatus(request.status); 
        _repository.Update(order);
        await _unitOfWork.CommitAsync(cancellationToken);
        
        return Result.Success(_mapper.Map<OrderDto>(order));
    }
}
