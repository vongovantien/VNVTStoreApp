using Microsoft.AspNetCore.SignalR;

namespace VNVTStore.Infrastructure.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveOrderNotification", message);
    }

    public async Task BroadcastSystemMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveSystemNotification", message);
    }
}
