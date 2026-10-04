using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Interfaces;
using VNVTStore.Infrastructure.Templates;

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
            _logger.LogWarning("[SendEmailAsync] Email settings missing. Email to {To} not sent.", to);
            return;
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new System.Net.NetworkCredential(fromEmail, password),
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
            _logger.LogInformation("[SendEmailAsync] Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SendEmailAsync] Failed to send email to {To}", to);
            throw;
        }
    }

    /// <summary>
    /// Gửi email xác nhận đăng ký với template đẹp
    /// </summary>
    public async Task SendVerificationEmailAsync(string to, string userName, string verificationLink)
    {
        var html = EmailTemplates.EmailVerification(userName, verificationLink);
        await SendEmailAsync(to, "Xác nhận tài khoản VNVTStore", html, isHtml: true);
    }

    /// <summary>
    /// Gửi email reset password với template đẹp
    /// </summary>
    public async Task SendPasswordResetEmailAsync(string to, string userName, string resetLink)
    {
        var html = EmailTemplates.PasswordReset(userName, resetLink);
        await SendEmailAsync(to, "Đặt lại mật khẩu VNVTStore", html, isHtml: true);
    }

    /// <summary>
    /// Gửi email xác nhận đơn hàng với template đẹp
    /// </summary>
    public async Task SendOrderConfirmationEmailAsync(string to, string customerName, 
        string orderNumber, decimal totalAmount, 
        List<(string Name, int Quantity, decimal Price)> items, string orderViewLink)
    {
        var itemsHtml = EmailTemplates.BuildOrderItemsHtml(items);
        var html = EmailTemplates.OrderConfirmation(customerName, orderNumber, totalAmount, itemsHtml, orderViewLink);
        await SendEmailAsync(to, $"Xác nhận đơn hàng #{orderNumber}", html, isHtml: true);
    }
}
