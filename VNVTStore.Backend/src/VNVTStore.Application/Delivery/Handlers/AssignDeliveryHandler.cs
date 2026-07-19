using AutoMapper;
using MediatR;
using System.Linq;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.Delivery.Commands;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Delivery.Handlers;

public class AssignDeliveryHandler : BaseHandler<TblDelivery>, IRequestHandler<AssignDeliveryCommand, Result<DeliveryDto>>
{
    private readonly IRepository<TblOrder> _orderRepository;
    private readonly INotificationService _notificationService;

    public AssignDeliveryHandler(
        IRepository<TblDelivery> repository,
        IRepository<TblOrder> orderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        INotificationService notificationService) : base(repository, unitOfWork, mapper, dapperContext)
    {
        _orderRepository = orderRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<DeliveryDto>> Handle(AssignDeliveryCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByCodeAsync(request.OrderCode, cancellationToken);
        if (order == null)
        {
            return Result.Failure<DeliveryDto>(Error.NotFound(MessageConstants.Order, request.OrderCode));
        }

        if (order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Pending)
        {
            return Result.Failure<DeliveryDto>(Error.Validation("Delivery", "Chỉ có thể giao đơn hàng ở trạng thái Đã xác nhận hoặc Chờ xử lý."));
        }

        // Check if delivery already exists
        var existingDelivery = await _repository.FindAsync(d => d.OrderCode == request.OrderCode, cancellationToken);
        if (existingDelivery != null)
        {
            return Result.Failure<DeliveryDto>(Error.Validation("Delivery", "Đơn hàng này đã được tạo phiếu giao hàng."));
        }

        var delivery = new TblDelivery
        {
            OrderCode = request.OrderCode,
            ShipperCode = request.Dto.ShipperCode,
            ShipperName = request.Dto.ShipperName,
            ShipperPhone = request.Dto.ShipperPhone,
            TrackingNumber = request.Dto.TrackingNumber,
            CarrierName = string.IsNullOrEmpty(request.Dto.CarrierName) ? "Nội bộ" : request.Dto.CarrierName,
            EstimatedDeliveryDate = request.Dto.EstimatedDeliveryDate,
            Note = request.Dto.Note,
            Status = DeliveryStatus.Assigned
        };

        var history = new TblDeliveryHistory
        {
            Status = DeliveryStatus.Assigned,
            Note = request.Dto.Note ?? "Tạo phiếu giao hàng",
            UpdatedByCode = request.UserCode
        };

        delivery.TblDeliveryHistories.Add(history);
        
        await _repository.AddAsync(delivery, cancellationToken);
        
        // Update Order
        order.TrackingNumber = request.Dto.TrackingNumber;
        order.EstimatedDeliveryDate = request.Dto.EstimatedDeliveryDate;
        order.UpdateStatus(OrderStatus.Shipped);
        _orderRepository.Update(order);
        
        await _unitOfWork.CommitAsync(cancellationToken);

        // Notify user
        if (!string.IsNullOrEmpty(order.UserCode) && order.UserCode != "USR_GUEST")
        {
             await _notificationService.SendToUserAsync(order.UserCode, 
                 "Đơn hàng đang giao", 
                 $"Đơn hàng #{order.Code} của bạn đã được chuyển cho đơn vị vận chuyển.", 
                 "DELIVERY", 
                 $"/tracking?code={order.Code}");
        }

        var resultDto = _mapper.Map<DeliveryDto>(delivery);
        return Result.Success(resultDto);
    }
}
