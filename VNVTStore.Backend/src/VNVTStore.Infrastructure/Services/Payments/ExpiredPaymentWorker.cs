using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Payments.Commands;

namespace VNVTStore.Infrastructure.Services.Payments;

/// <summary>
/// Background worker that periodically checks for expired online payments,
/// cancels the pending orders, and restores product stock.
/// </summary>
public class ExpiredPaymentWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredPaymentWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public ExpiredPaymentWorker(IServiceScopeFactory scopeFactory, ILogger<ExpiredPaymentWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ExpiredPaymentWorker] Started expired payment cleanup worker.");

        // Initial delay to let the app fully initialize
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var cancelledCount = await mediator.Send(new CancelExpiredOnlinePaymentsCommand(), stoppingToken);
                if (cancelledCount > 0)
                {
                    _logger.LogInformation("[ExpiredPaymentWorker] Auto-cancelled {Count} expired online orders and restored stock.", cancelledCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ExpiredPaymentWorker] Error occurred while checking for expired payments.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
