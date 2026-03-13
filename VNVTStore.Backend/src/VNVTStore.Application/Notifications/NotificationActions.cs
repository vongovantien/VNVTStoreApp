using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Notifications.Queries
{
    public record GetMyNotificationsQuery() : IRequest<Result<List<NotificationDto>>>;
}

namespace VNVTStore.Application.Notifications.Commands
{
    public record MarkAsReadCommand(string Code) : IRequest<Result<bool>>;
    public record CreateNotificationCommand(string UserCode, string Title, string Message, string Type = "INFO", string? Link = null) : IRequest<Result<string>>;
}
