using System.Threading.Tasks;

namespace VNVTStore.Application.Interfaces;

public interface INotificationService
{
    Task SendAsync(string method, object message);
}
