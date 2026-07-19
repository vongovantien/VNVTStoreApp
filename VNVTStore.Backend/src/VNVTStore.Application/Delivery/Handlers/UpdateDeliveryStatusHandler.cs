using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.Delivery.Commands;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Delivery.Handlers;

public class UpdateDeliveryStatusHandler : BaseHandler<TblDelivery>, IRequestHandler<UpdateDeliveryStatusCommand, Result<DeliveryDto>>
{
    private readonly IRepository<TblOrder> _orderRepository;
    private readonly INotificationService _notificationService;

    public UpdateDeliveryStatusHandler(
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

    public async Task<Result<DeliveryDto>> Handle(UpdateDeliveryStatusCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _repository.GetByCodeAsync(request.DeliveryCode, cancellationToken);
        if (delivery == null)
        {
            return Result.Failure<DeliveryDto>(Error.NotFound("Delivery", request.DeliveryCode));
        }

        if (!Enum.TryParse<DeliveryStatus>(request.Status, true, out var newStatus))
        {
            return Result.Failure<DeliveryDto>(Error.Validation("Delivery", "Trạng thái không hợp lệ."));
        }

        delivery.Status = newStatus;
        if (newStatus == DeliveryStatus.PickedUp) delivery.PickedUpAt = DateTime.UtcNow;
        if (newStatus == DeliveryStatus.Delivered) delivery.DeliveredAt = DateTime.UtcNow;

        var history = new TblDeliveryHistory
        {
            Status = newStatus,
            Note = request.Note,
            Location = request.Location,
            UpdatedByCode = request.UserCode
        };

        delivery.TblDeliveryHistories.Add(history);
        _repository.Update(delivery);

        // Synchronize Order status
        var order = await _orderRepository.GetByCodeAsync(delivery.OrderCode, cancellationToken);
        if (order != null)
        {
            if (newStatus == DeliveryStatus.Delivered)
            {
                order.UpdateStatus(OrderStatus.Delivered);
            }
            else if (newStatus == DeliveryStatus.Returned)
            {
                order.UpdateStatus(OrderStatus.Cancelled);
            }
            _orderRepository.Update(order);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        // Notify user
        if (order != null && !string.IsNullOrEmpty(order.UserCode) && order.UserCode != "USR_GUEST")
        {
            string message = newStatus switch
            {
                DeliveryStatus.OutForDelivery => $"Đơn hàng #{order.Code} đang được giao đến bạn. Vui lòng chú ý điện thoại.",
                DeliveryStatus.Delivered => $"Đơn hàng #{order.Code} đã được giao thành công.",
                DeliveryStatus.Failed => $"Giao đơn hàng #{order.Code} thất bại. Cửa hàng sẽ liên hệ lại với bạn.",
                _ => $"Cập nhật trạng thái giao hàng: {newStatus}"
            };

            await _notificationService.SendToUserAsync(order.UserCode, 
                "Cập nhật giao hàng", 
                message, 
                "DELIVERY", 
                $"/tracking?code={order.Code}");
        }

        var resultDto = _mapper.Map<DeliveryDto>(delivery);
        return Result.Success(resultDto);
    }
}
