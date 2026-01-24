using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using VNVTStore.Infrastructure.Hubs;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
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
        await _hubContext.Clients.All.SendAsync("ReceiveSystemNotification", new { Key = key, Args = args });
    }
}
