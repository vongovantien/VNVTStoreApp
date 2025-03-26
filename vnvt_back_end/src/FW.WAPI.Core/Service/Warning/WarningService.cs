using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.Email;
using FW.WAPI.Core.DAL.Model.MediaType;
using FW.WAPI.Core.DAL.Model.Slack;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Infrastructure.Logger;
using FW.WAPI.Core.Runtime.Session;
using FW.WAPI.Core.Service.Notification;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.Service.Warning
{
    public class WarningService : IWarningService
    {
        private readonly EmailConfig _emailConfig;
        private readonly SlackConfig _slackConfig;
        private readonly int EMAIL_TIMEOUT = 20000;
        private readonly int EMAIL_PORT = 587;
        private readonly string EMAIL_HOST = "smtp.gmail.com";
        private readonly string SLACK_HOST = "https://slack.com/api/chat.postMessage";
        private readonly ILogger _logger;
        private readonly ICoreLogger _coreLogger;
        private readonly IBaseSession _baseSession;
        private readonly INotifyService _notifyService;
        public WarningService(IOptions<EmailConfig> configuration,
            IOptions<SlackConfig> slackConfig,
            ILogger<WarningService> logger,
            IBaseSession baseSession,
            ICoreLogger coreLogger,
            INotifyService notifyService)
        {
            _emailConfig = configuration.Value;
            _slackConfig = slackConfig.Value;
            var decryptToken = Utilities.DecryptData(_slackConfig.Token, typeof(SlackConfig).Name);
            if (!string.IsNullOrEmpty(decryptToken)) _slackConfig.Token = decryptToken;

            _logger = logger;
            _coreLogger = coreLogger;
            _baseSession = baseSession;
            _notifyService = notifyService;
        }

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
        public async Task<bool> SendEmail(string email, string name, string password, string[] to, string subject, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(email)) email = _emailConfig.Email;
                if (string.IsNullOrEmpty(password)) password = _emailConfig.Password;
                if (string.IsNullOrEmpty(name)) name = _emailConfig.Name;

                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(email, name);

                SmtpClient client = new SmtpClient();

                client.Timeout = 20000;
                client.Port = 587;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(email, password);
                client.Host = EMAIL_HOST;

                foreach (var item in to)
                {
                    mail.To.Add(item);
                }

                mail.Subject = subject;
                mail.Body = body;

                await client.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot send email, ex:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="body"></param>
        public async Task<bool> SendEmail(string body)
        {
            try
            {
                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(_emailConfig.Email, _emailConfig.Name);

                SmtpClient client = new SmtpClient();

                client.Timeout = EMAIL_TIMEOUT;
                client.Port = EMAIL_PORT;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(_emailConfig.Email, _emailConfig.Password);
                client.Host = "smtp.gmail.com";

                foreach (var item in _emailConfig.To)
                {
                    mail.To.Add(item);
                }

                mail.Subject = _emailConfig.Subject;
                mail.Body = body;

                await client.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot send email, ex:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task SendSlackMessage(string text, string channel = null)
        {
            try
            {
                var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_slackConfig.Token}");

                var message = new SlackMessage()
                {
                    Channel = channel ?? _slackConfig.Channel,
                    Text = text
                };

                var body = new StringContent(JsonUtilities.ConvertObjectToJson(message), Encoding.UTF8, MediaTypeName.JSON);

                var response = await httpClient.PostAsync(SLACK_HOST, body);

                if (!response.IsSuccessStatusCode)
                {
                    _coreLogger.Log(LogLevel.Warning, $"Send Slack message fail: {JsonUtilities.ConvertObjectToJson(response)}");
                }
            }
            catch (System.Exception ex)
            {
                _coreLogger.Log(LogLevel.Error, $"Error Send Slack message: {JsonUtilities.ConvertObjectToJson(text)}, Exception: {ex.ToString()}");
            }
        }

        /// <summary>
        /// Send Slack Warning
        /// </summary>
        /// <param name="message"></param>
        /// <param name="companyCode"></param>
        /// <param name="channel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<NotifyResult> SendSlackWarning(string message, string companyCode = null,
                                        string channel = null, string tenantCode = null)
        {
            try
            {
                if (tenantCode == null) tenantCode = _baseSession.TenantCode;
                if (channel == null) channel = _slackConfig.Channel;

                var notifyParam = new NotificationParamDTO((short)NotifyMethod.Slack,
                    (short)NotifyType.SystemWarning, companyCode, message, tenantCode)
                {
                    Subject = "Cảnh báo",
                    Recipient = channel,
                    IsSkipLog = true
                };

                var result = await _notifyService.SendNotification(notifyParam);
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
