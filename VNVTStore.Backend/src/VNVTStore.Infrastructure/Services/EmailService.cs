using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ISecretConfigurationService _secretConfig;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ISecretConfigurationService secretConfig, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _secretConfig = secretConfig;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        var host = await _secretConfig.GetSecretAsync("EMAIL_HOST") ?? _configuration["EmailSettings:Host"];
        var portStr = await _secretConfig.GetSecretAsync("EMAIL_PORT") ?? _configuration["EmailSettings:Port"];
        var port = int.Parse(portStr ?? "587");
        var fromEmail = await _secretConfig.GetSecretAsync("EMAIL_FROM") ?? _configuration["EmailSettings:FromEmail"];
        var password = await _secretConfig.GetSecretAsync("EMAIL_PASSWORD") ?? _configuration["EmailSettings:Password"];
        var enableSslStr = await _secretConfig.GetSecretAsync("EMAIL_SSL") ?? _configuration["EmailSettings:EnableSsl"];
        var enableSsl = bool.Parse(enableSslStr ?? "true");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(fromEmail))
        {
            _logger.LogWarning("[SendEmailAsync] error: Email settings are missing. Email to {To} with subject {Subject} was not sent.", to, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("[SendEmailAsync] Email sent to {To} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SendEmailAsync] error: Failed to send email to {To}", to);
            // We might not want to throw here to avoid failing the main request if email is secondary
            // But for verification it is critical.
            throw; 
        }
    }
}
