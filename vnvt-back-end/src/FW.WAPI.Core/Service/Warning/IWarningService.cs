
using FW.WAPI.Core.DAL.DTO;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Service.Warning
{
    public interface IWarningService
    {
        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task<bool> SendEmail(string email, string name, string password, string[] to, string subject, string body);

        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        Task<bool> SendEmail(string body);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        Task SendSlackMessage(string text, string channel = null);

        /// <summary>
        /// Send Slack Warning
        /// </summary>
        /// <param name="message"></param>
        /// <param name="companyCode"></param>
        /// <param name="channel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<NotifyResult> SendSlackWarning(string message, string companyCode = null,
                                      string channel = null, string tenantCode = null);
    }
}
