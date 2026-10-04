using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Notifications.Commands;
using VNVTStore.Application.Notifications.Queries;
using VNVTStore.Domain.Common;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Notifications;

public class NotificationHandlers : 
    IRequestHandler<GetMyNotificationsQuery, Result<List<NotificationDto>>>,
    IRequestHandler<GetUnreadCountQuery, Result<int>>,
    IRequestHandler<MarkAsReadCommand, Result<bool>>,
    IRequestHandler<MarkAllAsReadCommand, Result<bool>>,
    IRequestHandler<DeleteNotificationCommand, Result<bool>>,
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

    public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode)) return Result.Success(0);

        var unread = await _repository.FindAllAsync(n => n.UserCode == userCode && n.IsActive && !n.IsRead, cancellationToken);
        return Result.Success(unread.Count());
    }

    public async Task<Result<bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (notification == null) return Result.Failure<bool>("Notification not found");

        var userCode = _currentUser.UserCode;
        if (!string.IsNullOrEmpty(userCode) && notification.UserCode != userCode)
        {
            return Result.Failure<bool>("Unauthorized");
        }

        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;

        _repository.Update(notification);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<bool>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode)) return Result.Failure<bool>("Unauthorized");

        var unreadNotifications = await _repository.FindAllAsync(n => n.UserCode == userCode && n.IsActive && !n.IsRead, cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var notif in unreadNotifications)
        {
            notif.IsRead = true;
            notif.UpdatedAt = now;
            _repository.Update(notif);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(true);
    }

    public async Task<Result<bool>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (notification == null) return Result.Failure<bool>("Notification not found");

        var userCode = _currentUser.UserCode;
        if (!string.IsNullOrEmpty(userCode) && notification.UserCode != userCode)
        {
            return Result.Failure<bool>("Unauthorized");
        }

        notification.IsActive = false;
        notification.UpdatedAt = DateTime.UtcNow;

        _repository.Update(notification);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<string>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new TblNotification
        {
            Code = CodeGenerator.New(),
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
