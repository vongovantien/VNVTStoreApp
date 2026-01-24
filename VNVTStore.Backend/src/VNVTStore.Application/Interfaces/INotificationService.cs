using System.Threading.Tasks;

namespace VNVTStore.Application.Interfaces;

public interface INotificationService
{
    Task SendAsync(string method, object message);
    Task BroadcastMessageAsync(string message);
    Task BroadcastLocalizedAsync(string key, params object[] args);
}
