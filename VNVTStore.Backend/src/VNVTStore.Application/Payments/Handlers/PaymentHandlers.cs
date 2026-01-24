using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Payments.Commands;
using VNVTStore.Application.Payments.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Payments.Handlers;

public class PaymentHandlers :
    IRequestHandler<ProcessPaymentCommand, Result<PaymentDto>>,
    IRequestHandler<UpdatePaymentStatusCommand, Result<PaymentDto>>,
    IRequestHandler<GetPaymentByOrderQuery, Result<PaymentDto>>,
    IRequestHandler<GetMyPaymentsQuery, Result<IEnumerable<PaymentDto>>>
{
    private readonly IRepository<TblPayment> _paymentRepository;
    private readonly IRepository<TblOrder> _orderRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentHandlers(
        IRepository<TblPayment> paymentRepository,
        IRepository<TblOrder> orderRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PaymentDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByCodeAsync(request.orderCode, cancellationToken);
        if (order == null)
            return Result.Failure<PaymentDto>(Error.NotFound(MessageConstants.Order, request.orderCode));

        if (!string.IsNullOrEmpty(_currentUser.UserCode) && order.UserCode != _currentUser.UserCode)
            return Result.Failure<PaymentDto>(Error.Forbidden("Cannot pay for another user's order"));

        var payment = TblPayment.Create(
            request.orderCode,
            request.amount,
            request.paymentMethod
        );

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<PaymentDto>(payment));
    }

    public async Task<Result<PaymentDto>> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByCodeAsync(request.paymentCode, cancellationToken);
        if (payment == null)
            return Result.Failure<PaymentDto>(Error.NotFound(MessageConstants.Payment, request.paymentCode));

        payment.UpdateStatus(request.status, request.transactionId);
        
        // Update Order status if payment completed
        if (request.status == PaymentStatus.Completed)
        {
            var order = await _orderRepository.GetByCodeAsync(payment.OrderCode!, cancellationToken);
            if (order != null)
            {
                order.UpdateStatus(OrderStatus.Paid); 
                _orderRepository.Update(order);
            }
        }

        _paymentRepository.Update(payment);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<PaymentDto>(payment));
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentByOrderQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.AsQueryable()
            .FirstOrDefaultAsync(p => p.OrderCode == request.orderCode, cancellationToken);

        if (payment == null)
            return Result.Failure<PaymentDto>(Error.NotFound(MessageConstants.Payment, request.orderCode));

        // Check ownership via order
        var order = await _orderRepository.GetByCodeAsync(request.orderCode, cancellationToken);
        if (order == null)
             return Result.Failure<PaymentDto>(Error.NotFound(MessageConstants.Order, request.orderCode));

        if (!string.IsNullOrEmpty(_currentUser.UserCode) && order.UserCode != _currentUser.UserCode)
             return Result.Failure<PaymentDto>(Error.Forbidden("Cannot view payment of another user"));

        return Result.Success(_mapper.Map<PaymentDto>(payment));
    }

    public async Task<Result<IEnumerable<PaymentDto>>> Handle(GetMyPaymentsQuery request, CancellationToken cancellationToken)
    {
        var userCode = _currentUser.UserCode;
        // Join with Order to filter by currentUser
        var payments = await _paymentRepository.AsQueryable()
            .Include(p => p.OrderCodeNavigation)
            .Where(p => p.OrderCodeNavigation.UserCode == userCode)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<IEnumerable<PaymentDto>>(payments));
    }
}
