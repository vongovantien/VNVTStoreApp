using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    IRequestHandler<GetMyPaymentsQuery, Result<IEnumerable<PaymentDto>>>,
    IRequestHandler<GetAllPaymentsQuery, Result<PagedResult<PaymentDto>>>,
    IRequestHandler<CreatePaymentUrlCommand, Result<PaymentUrlDto>>,
    IRequestHandler<ConfirmGatewayPaymentCommand, Result<PaymentConfirmationDto>>,
    IRequestHandler<CancelExpiredOnlinePaymentsCommand, int>
{
    private readonly IRepository<TblPayment> _paymentRepository;
    private readonly IRepository<TblOrder> _orderRepository;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly ISecretConfigurationService _secretConfig;
    private readonly IBaseUrlService _baseUrlService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PaymentHandlers> _logger;

    public PaymentHandlers(
        IRepository<TblPayment> paymentRepository,
        IRepository<TblOrder> orderRepository,
        IRepository<TblProduct> productRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPaymentGatewayFactory gatewayFactory,
        ISecretConfigurationService secretConfig,
        IBaseUrlService baseUrlService,
        INotificationService notificationService,
        ILogger<PaymentHandlers> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _gatewayFactory = gatewayFactory;
        _secretConfig = secretConfig;
        _baseUrlService = baseUrlService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByCodeAsync(request.orderCode, cancellationToken);
        if (order == null)
            return Result.Failure<PaymentDto>(Error.NotFound(MessageConstants.Order, request.orderCode));

        if (!string.IsNullOrEmpty(_currentUser.UserCode) && order.UserCode != _currentUser.UserCode)
            return Result.Failure<PaymentDto>(Error.Forbidden("Cannot pay for another user's order"));

        var existingPayment = await _paymentRepository.AsQueryable()
            .FirstOrDefaultAsync(p => p.OrderCode == request.orderCode, cancellationToken);

        if (existingPayment != null)
        {
            if (existingPayment.Status == PaymentStatus.Completed)
                return Result.Failure<PaymentDto>(Error.Conflict("Order has already been paid"));

            existingPayment.Retry(request.paymentMethod, order.FinalAmount);
            _paymentRepository.Update(existingPayment);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(_mapper.Map<PaymentDto>(existingPayment));
        }

        var payment = TblPayment.Create(
            request.orderCode,
            order.FinalAmount,
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
                order.MarkPaid();
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
        var payments = await _paymentRepository.AsQueryable()
            .Include(p => p.OrderCodeNavigation)
            .Where(p => p.OrderCodeNavigation.UserCode == userCode)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<IEnumerable<PaymentDto>>(payments));
    }

    public async Task<Result<PagedResult<PaymentDto>>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _paymentRepository.AsQueryable()
            .OrderByDescending(p => p.PaymentDate);

        var payments = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var count = await query.CountAsync(cancellationToken);
        var dtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

        return Result.Success(new PagedResult<PaymentDto>(dtos, count, request.PageIndex, request.PageSize));
    }

    public async Task<Result<PaymentUrlDto>> Handle(CreatePaymentUrlCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByCodeAsync(request.OrderCode, cancellationToken);
        if (order == null)
            return Result.Failure<PaymentUrlDto>(Error.NotFound(MessageConstants.Order, request.OrderCode));

        if (!string.IsNullOrEmpty(_currentUser.UserCode) && order.UserCode != _currentUser.UserCode && _currentUser.UserCode != "USR_GUEST")
            return Result.Failure<PaymentUrlDto>(Error.Forbidden("Cannot pay for another user's order"));

        if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Completed)
            return Result.Failure<PaymentUrlDto>(Error.Conflict("Order has already been paid"));

        if (order.Status == OrderStatus.Cancelled)
            return Result.Failure<PaymentUrlDto>(Error.Conflict("Order has been cancelled"));

        var payment = await _paymentRepository.AsQueryable()
            .FirstOrDefaultAsync(p => p.OrderCode == request.OrderCode, cancellationToken);

        var targetMethod = request.PaymentMethod ?? (payment != null ? payment.Method : PaymentMethod.VnPay);

        var gateway = _gatewayFactory.Get(targetMethod);
        if (gateway == null)
            return Result.Failure<PaymentUrlDto>(Error.Validation($"Unsupported payment method: {targetMethod}"));

        if (!await gateway.IsConfiguredAsync())
            return Result.Failure<PaymentUrlDto>(Error.Validation($"Gateway {targetMethod} is not properly configured."));

        if (payment == null)
        {
            payment = TblPayment.Create(order.Code, order.FinalAmount, targetMethod);
            await _paymentRepository.AddAsync(payment, cancellationToken);
        }
        else
        {
            if (payment.Status == PaymentStatus.Completed)
                return Result.Failure<PaymentUrlDto>(Error.Conflict("Order has already been paid"));

            payment.Retry(targetMethod, order.FinalAmount);
            _paymentRepository.Update(payment);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        var txnRef = PaymentReference.Build(payment.Code, DateTime.UtcNow);

        var frontendUrl = await _secretConfig.GetSecretAsync(PaymentSecretKeys.FrontendUrl) ?? "http://localhost:5173";
        var customReturnUrl = await _secretConfig.GetSecretAsync(PaymentSecretKeys.ReturnUrl);
        var returnUrl = !string.IsNullOrWhiteSpace(customReturnUrl)
            ? customReturnUrl
            : $"{frontendUrl.TrimEnd('/')}/payment/result";

        var ipnBaseUrl = await _secretConfig.GetSecretAsync(PaymentSecretKeys.IpnBaseUrl);
        if (string.IsNullOrWhiteSpace(ipnBaseUrl))
        {
            ipnBaseUrl = _baseUrlService.GetBaseUrl();
        }

        var ipnUrl = $"{ipnBaseUrl.TrimEnd('/')}/api/v1/payments/{targetMethod.ToString().ToLower()}/ipn";

        var gatewayReq = new PaymentGatewayRequest(
            TxnRef: txnRef,
            Amount: order.FinalAmount,
            OrderCode: order.Code,
            OrderInfo: $"Thanh toan don hang {order.Code}",
            ClientIp: string.IsNullOrWhiteSpace(request.ClientIp) ? "127.0.0.1" : request.ClientIp,
            ReturnUrl: returnUrl,
            IpnUrl: ipnUrl
        );

        var paymentUrl = await gateway.CreatePaymentUrlAsync(gatewayReq, cancellationToken);

        return Result.Success(new PaymentUrlDto
        {
            OrderCode = order.Code,
            PaymentCode = payment.Code,
            PaymentUrl = paymentUrl
        });
    }

    public async Task<Result<PaymentConfirmationDto>> Handle(ConfirmGatewayPaymentCommand request, CancellationToken cancellationToken)
    {
        var gateway = _gatewayFactory.Get(request.Gateway);
        if (gateway == null)
        {
            return Result.Success(new PaymentConfirmationDto
            {
                Outcome = PaymentConfirmOutcome.NotFound,
                Message = $"Unsupported gateway {request.Gateway}"
            });
        }

        var verification = await gateway.VerifyCallbackAsync(request.Data);
        if (!verification.IsValidSignature)
        {
            _logger.LogWarning("[PaymentConfirm] Invalid signature from {Gateway}", request.Gateway);
            return Result.Success(new PaymentConfirmationDto
            {
                Outcome = PaymentConfirmOutcome.InvalidSignature,
                ResponseCode = verification.ResponseCode,
                Message = "Invalid signature"
            });
        }

        string? paymentCode = null;
        if (!string.IsNullOrWhiteSpace(verification.TxnRef))
        {
            if (PaymentReference.TryParse(verification.TxnRef, out var parsedCode))
            {
                paymentCode = parsedCode;
            }
            else
            {
                paymentCode = verification.TxnRef;
            }
        }

        var payment = await _paymentRepository.AsQueryable()
            .Include(p => p.OrderCodeNavigation)
            .FirstOrDefaultAsync(p => p.Code == paymentCode || p.OrderCode == paymentCode || p.OrderCode == verification.TxnRef, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("[PaymentConfirm] Payment not found for reference: {TxnRef}", verification.TxnRef);
            return Result.Success(new PaymentConfirmationDto
            {
                Outcome = PaymentConfirmOutcome.NotFound,
                ResponseCode = verification.ResponseCode,
                Message = "Payment not found"
            });
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            return Result.Success(new PaymentConfirmationDto
            {
                Outcome = PaymentConfirmOutcome.AlreadyConfirmed,
                OrderCode = payment.OrderCode,
                PaymentCode = payment.Code,
                Amount = payment.Amount,
                ResponseCode = verification.ResponseCode,
                Message = "Payment was already confirmed"
            });
        }

        if (!verification.IsSuccess)
        {
            _logger.LogInformation("[PaymentConfirm] Gateway reported failure for payment {Code}. Code: {ResponseCode}", payment.Code, verification.ResponseCode);
            payment.MarkFailed(verification.GatewayTransactionId);
            _paymentRepository.Update(payment);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(new PaymentConfirmationDto
            {
                Outcome = PaymentConfirmOutcome.Failed,
                OrderCode = payment.OrderCode,
                PaymentCode = payment.Code,
                Amount = payment.Amount,
                ResponseCode = verification.ResponseCode,
                Message = verification.Message ?? "Payment failed or was cancelled"
            });
        }

        if (Math.Abs(payment.Amount - verification.Amount) > 1)
        {
            _logger.LogWarning("[PaymentConfirm] Amount mismatch for {Code}: expected {Expected}, got {Actual}",
                payment.Code, payment.Amount, verification.Amount);

            return Result.Success(new PaymentConfirmationDto
            {
                Outcome = PaymentConfirmOutcome.InvalidAmount,
                OrderCode = payment.OrderCode,
                PaymentCode = payment.Code,
                Amount = verification.Amount,
                ResponseCode = verification.ResponseCode,
                Message = "Amount mismatch"
            });
        }

        payment.MarkCompleted(verification.GatewayTransactionId);
        _paymentRepository.Update(payment);

        var order = await _orderRepository.GetByCodeAsync(payment.OrderCode, cancellationToken);
        if (order != null)
        {
            order.MarkPaid();
            _orderRepository.Update(order);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        // Notify admins & user
        try
        {
            await _notificationService.SendAsync("ReceiveOrderNotification", new
            {
                Message = $"Đơn hàng #{payment.OrderCode} đã thanh toán online thành công ({payment.Amount:N0} đ)"
            });

            if (order != null && !string.IsNullOrEmpty(order.UserCode) && order.UserCode != "USR_GUEST")
            {
                await _notificationService.SendToUserAsync(
                    order.UserCode,
                    "Thanh toán thành công",
                    $"Đơn hàng #{order.Code} đã được thanh toán thành công qua {request.Gateway}.",
                    "SUCCESS",
                    $"/account/orders/{order.Code}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast payment notification for {OrderCode}", payment.OrderCode);
        }

        return Result.Success(new PaymentConfirmationDto
        {
            Outcome = PaymentConfirmOutcome.Confirmed,
            OrderCode = payment.OrderCode,
            PaymentCode = payment.Code,
            Amount = payment.Amount,
            ResponseCode = verification.ResponseCode,
            Message = "Payment confirmed successfully"
        });
    }

    public async Task<int> Handle(CancelExpiredOnlinePaymentsCommand request, CancellationToken cancellationToken)
    {
        var timeoutStr = await _secretConfig.GetSecretAsync(PaymentSecretKeys.TimeoutMinutes);
        if (!int.TryParse(timeoutStr, out var timeoutMinutes) || timeoutMinutes <= 0)
        {
            timeoutMinutes = 30;
        }

        var cutoff = DateTime.UtcNow.AddMinutes(-timeoutMinutes);

        var expiredOrders = await _orderRepository.AsQueryable()
            .Include(o => o.TblPayment)
            .Include(o => o.TblOrderItems)
                .ThenInclude(oi => oi.ProductCodeNavigation)
            .Where(o => o.Status == OrderStatus.Pending &&
                        o.CreatedAt < cutoff &&
                        o.TblPayment != null &&
                        (o.TblPayment.Method == PaymentMethod.VnPay || o.TblPayment.Method == PaymentMethod.MoMo) &&
                        o.TblPayment.Status == PaymentStatus.Pending)
            .ToListAsync(cancellationToken);

        if (!expiredOrders.Any()) return 0;

        foreach (var order in expiredOrders)
        {
            _logger.LogInformation("[AutoCancel] Expired online order {Code} timed out after {Minutes}m", order.Code, timeoutMinutes);

            try
            {
                order.Cancel($"Quá thời gian thanh toán online ({timeoutMinutes} phút)");
                _orderRepository.Update(order);

                if (order.TblPayment != null)
                {
                    order.TblPayment.UpdateStatus(PaymentStatus.Cancelled, "TIMEOUT");
                    _paymentRepository.Update(order.TblPayment);
                }

                foreach (var item in order.TblOrderItems)
                {
                    if (item.ProductCodeNavigation != null)
                    {
                        item.ProductCodeNavigation.RestoreStock(item.Quantity);
                        _productRepository.Update(item.ProductCodeNavigation);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AutoCancel] Failed to cancel order {Code}", order.Code);
            }
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return expiredOrders.Count;
    }
}
