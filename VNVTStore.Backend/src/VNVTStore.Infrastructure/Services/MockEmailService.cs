using VNVTStore.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace VNVTStore.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        _logger.LogInformation($"[MockEmail] To: {to}, Subject: {subject}");
        _logger.LogInformation($"[MockEmail] Body: {body}");
        return Task.CompletedTask;
    }
}
