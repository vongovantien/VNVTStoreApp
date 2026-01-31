using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Orders.Handlers;

public class VerifyOrderHandler : BaseHandler<TblOrder>, 
    IRequestHandler<VerifyOrderCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public VerifyOrderHandler(
        IRepository<TblOrder> orderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IApplicationDbContext context) : base(orderRepository, unitOfWork, mapper, dapperContext)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(VerifyOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.TblOrders.FirstOrDefaultAsync(o => o.VerificationToken == request.Token, cancellationToken);
        
        if (order == null)
        {
            return Result.Failure<string>(Error.Validation("Invalid verification token."));
        }

        if (order.VerificationTokenExpiresAt < DateTime.UtcNow)
        {
             return Result.Failure<string>(Error.Validation("Verification token has expired."));
        }

        if (order.Status != OrderStatus.Pending)
        {
             return Result.Success("Order is already verified or processed.");
        }

        order.UpdateStatus(OrderStatus.Confirmed); 
        order.SetVerificationToken(null!, DateTime.MinValue); 
        
        _repository.Update(order);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(order.Code);
    }
}
