using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Common.Helpers;

namespace VNVTStore.Application.Orders.Handlers;

public class CancelOrderHandler : BaseHandler<TblOrder>, 
    IRequestHandler<CancelOrderCommand, Result<bool>>
{
    private readonly IRepository<TblProduct> _productRepository;

    public CancelOrderHandler(
        IRepository<TblOrder> orderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IRepository<TblProduct> productRepository) : base(orderRepository, unitOfWork, mapper, dapperContext)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
             var order = await _repository.AsQueryable()
                  .Include(o => o.TblOrderItems)
                  .ThenInclude(oi => oi.ProductCodeNavigation)
                 .FirstOrDefaultAsync(o => o.Code == request.orderCode, cancellationToken);
    
            if (order == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<bool>(Error.NotFound(MessageConstants.Order, request.orderCode));
            }
    
            if (order.UserCode != request.userCode)
            {
                 await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                 return Result.Failure<bool>(Error.Forbidden("Cannot cancel another user's order"));
            }
    
            try 
            {
                order.Cancel(request.reason); 
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<bool>(Error.Validation(ex.Message));
            }
    
            foreach(var item in order.TblOrderItems)
            {
                item.ProductCodeNavigation.RestoreStock(item.Quantity);
                 _productRepository.Update(item.ProductCodeNavigation);
            }
    
            _repository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
    
            return Result.Success(true);
        }
        catch (Exception)
        {
             await _unitOfWork.RollbackTransactionAsync(cancellationToken);
             throw;
        }
    }
}
