using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace VNVTStore.Infrastructure.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userCode = Context.User?.FindFirst("userId")?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? Context.UserIdentifier;
            
        if (!string.IsNullOrEmpty(userCode))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userCode}");
        }

        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "Admin" || role == "Staff")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        await base.OnConnectedAsync();
    }

    public async Task SendNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveOrderNotification", message);
    }

    public async Task BroadcastSystemMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveSystemNotification", message);
    }

    public async Task SendQuoteNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveQuoteNotification", message);
    }
}
