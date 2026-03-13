using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Notifications.Commands;
using VNVTStore.Application.Notifications.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Notifications;

public class NotificationHandlers : 
    IRequestHandler<GetMyNotificationsQuery, Result<List<NotificationDto>>>,
    IRequestHandler<MarkAsReadCommand, Result<bool>>,
    IRequestHandler<CreateNotificationCommand, Result<string>>
{
    private readonly IRepository<TblNotification> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public NotificationHandlers(
        IRepository<TblNotification> repository, 
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICurrentUser currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<List<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode)) return Result.Failure<List<NotificationDto>>("Unauthorized");

        var notifications = await _repository.FindAllAsync(n => n.UserCode == userCode && n.IsActive, cancellationToken);
        return Result.Success(_mapper.Map<List<NotificationDto>>(notifications.OrderByDescending(n => n.CreatedAt)));
    }

    public async Task<Result<bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (notification == null) return Result.Failure<bool>("Notification not found");

        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;

        _repository.Update(notification);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<string>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new TblNotification
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            UserCode = request.UserCode,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            Link = request.Link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.AddAsync(notification, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(notification.Code);
    }
}
