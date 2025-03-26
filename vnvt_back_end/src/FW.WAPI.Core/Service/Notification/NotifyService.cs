using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.Notification;
using FW.WAPI.Core.Service.Remote;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Service.Notification
{
    public class NotifyService : RemoteService, INotifyService
    {
        private readonly NotificationConfig _notificationConfig;

        public NotifyService(HttpClient httpClient, IConfiguration configuration,
            IOptions<NotificationConfig> notificationConfig)
            : base(httpClient, configuration)
        {
            _notificationConfig = notificationConfig.Value;
        }

        /// <summary>
        /// Send Notification
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        public async Task<NotifyResult> SendNotification(NotificationParamDTO notification)
        {
            var url = _notificationConfig.Url;
            var result = await Post<NotifyResult>(url, notification);
            return result;
        }
    }

    public interface INotifyService
    {
        /// <summary>
        /// Send Notification
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        Task<NotifyResult> SendNotification(NotificationParamDTO notification);
    }
}