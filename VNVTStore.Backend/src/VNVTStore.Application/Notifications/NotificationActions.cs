using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Notifications.Queries
{
    public record GetMyNotificationsQuery() : IRequest<Result<List<NotificationDto>>>;
    public record GetUnreadCountQuery() : IRequest<Result<int>>;
}

namespace VNVTStore.Application.Notifications.Commands
{
    public record MarkAsReadCommand(string Code) : IRequest<Result<bool>>;
    public record MarkAllAsReadCommand() : IRequest<Result<bool>>;
    public record DeleteNotificationCommand(string Code) : IRequest<Result<bool>>;
    public record CreateNotificationCommand(string UserCode, string Title, string Message, string Type = "INFO", string? Link = null) : IRequest<Result<string>>;
}
