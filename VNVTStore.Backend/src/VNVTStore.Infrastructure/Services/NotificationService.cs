using Microsoft.AspNetCore.SignalR;
using MediatR;
using System.Threading.Tasks;
using VNVTStore.Infrastructure.Hubs;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IMediator _mediator;

    public NotificationService(IHubContext<NotificationHub> hubContext, IMediator mediator)
    {
        _hubContext = hubContext;
        _mediator = mediator;
    }

    public async Task SendAsync(string method, object message)
    {
        await _hubContext.Clients.All.SendAsync(method, message);
    }

    public async Task BroadcastMessageAsync(string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveSystemNotification", message);
    }

    public async Task BroadcastLocalizedAsync(string key, params object[] args)
    {
        // Persist notification for all admins or specific users if needed. 
        // For now, we broadcast to SignalR and could persist "Broadcast" type notifications to a global list if needed.
        await _hubContext.Clients.All.SendAsync("ReceiveSystemNotification", new { Key = key, Args = args });
    }

    public async Task SendToUserAsync(string userCode, string title, string message, string type = "INFO", string? link = null)
    {
        // 1. Persist to DB
        await _mediator.Send(new VNVTStore.Application.Notifications.Commands.CreateNotificationCommand(userCode, title, message, type, link));

        // 2. Notify via SignalR
        await _hubContext.Clients.User(userCode).SendAsync("ReceiveNotification", new { Title = title, Message = message, Type = type, Link = link });
    }
}
