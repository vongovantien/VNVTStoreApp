using FW.WAPI.Core.Domain.IntegrationEventLog;
using FW.WAPI.Core.Infrastructure.EventBus.Event;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Service.IntegrationEventLog
{
    public interface IIntegrationEventLogService
    {
        Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync();
        Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);
        Task SaveEventAsync(IntegrationEvent @event);
        Task MarkEventAsPublishedAsync(string eventId);
        Task MarkEventAsInProgressAsync(string eventId);
        Task MarkEventAsFailedAsync(string eventId);
        IntegrationEventLogContext IntegrationEventLogContext { get; set; }
        Task<IntegrationEventLogEntry> CheckEventExist(IntegrationEvent @event);
        Task<IntegrationEventLogEntry> CheckEventExist(string eventId);
        Task MarkEventAsProcessFailedAsync(string eventId);
        Task MarkEventAsProcessCompleteAsync(string eventId);
        Task SaveEventProcessing(IntegrationEvent @event);
        Task SaveEventProcessComplete(IntegrationEvent @event);
        Task SaveEventProcessComplete(IntegrationEvent @event, IDbContextTransaction transaction);
        Task SaveEventProcessFailed(IntegrationEvent @event);
        Task UpdateEventProcessing(IntegrationEvent @event);
    }
}
