using FW.WAPI.Core.Domain.IntegrationEventLog;
using FW.WAPI.Core.ExceptionHandling;
using FW.WAPI.Core.Infrastructure.EventBus.Event;
using FW.WAPI.Core.Infrastructure.Logger;
using FW.WAPI.Core.Service.Warning;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.Service.IntegrationEventLog
{
    public class IntegrationEventLogService : IIntegrationEventLogService
    {
        private readonly IntegrationEventLogContext _integrationEventLogContext;
        public IntegrationEventLogContext IntegrationEventLogContext { get; set; }
        private readonly List<Type> _eventTypes;
        private readonly ICoreLogger _logger;
        private readonly IWarningService _warningService;
        private readonly string _serviceName;
        public IntegrationEventLogService(IntegrationEventLogContext integrationEventLogContext, ICoreLogger logger,
            IWarningService warningService, IConfiguration configuration)
        {
            _integrationEventLogContext = integrationEventLogContext;
            IntegrationEventLogContext = _integrationEventLogContext;
            _eventTypes = Assembly.Load(Assembly.GetEntryAssembly().FullName)
                .GetTypes()
                .Where(t => t.Name.EndsWith(nameof(IntegrationEvent)))
                .ToList();
            _logger = logger;
            _warningService = warningService;
            _serviceName = configuration.GetSection("ServiceName").Value;
        }

        public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync()
        {
            return await _integrationEventLogContext.IntegrationEventLogs
                .Where(e => e.State == EventState.NotPublished.ToString())
                .OrderBy(o => o.CreationTime)
                .Select(e => e.DeserializeJsonContent(_eventTypes.Find(t => t.Name == e.EventTypeShortName)))
                .ToListAsync();
        }

        /// <summary>
        /// Save event with same transaction
        /// </summary>
        /// <param name="event"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction)
        {
            @event.ServiceName = _serviceName;
            var eventLogEntry = new IntegrationEventLogEntry(@event);
            _integrationEventLogContext.Database.UseTransaction(transaction.GetDbTransaction());
            _integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);

            return _integrationEventLogContext.SaveChangesAsync();
        }

        /// <summary>
        /// Save event without transaction
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task SaveEventAsync(IntegrationEvent @event)
        {
            @event.ServiceName = _serviceName;
            var eventLogEntry = new IntegrationEventLogEntry(@event);
            _integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);

            return _integrationEventLogContext.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public Task MarkEventAsPublishedAsync(string eventId)
        {
            return UpdateEventStatus(eventId, EventState.Published);
        }

        public Task MarkEventAsInProgressAsync(string eventId)
        {
            return UpdateEventStatus(eventId, EventState.InProgress);
        }

        public Task MarkEventAsFailedAsync(string eventId)
        {
            return UpdateEventStatus(eventId, EventState.PublishedFailed);
        }

        public Task MarkEventAsProcessFailedAsync(string eventId)
        {
            return UpdateEventStatus(eventId, EventState.ProcessFailed);
        }

        public Task MarkEventAsProcessCompleteAsync(string eventId)
        {
            return UpdateEventStatus(eventId, EventState.ProcessCompleted);
        }

        public Task MarkEventAsProcessingAsync(string eventId)
        {
            return UpdateEventStatus(eventId, EventState.Processing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task SaveEventProcessComplete(IntegrationEvent @event)
        {
            var alreadExist = await CheckEventExist(@event.Id.ToString());
            int i = 1;

            while (alreadExist == null)
            {
                alreadExist = await CheckEventExist(@event.Id.ToString());

                if (i > 5)
                {
                    _logger.Log(LogLevel.Error, $"SaveEventProcessComplete: Not fount event {@event.Id}");
                    throw new FriendlyException("Not found event to update");
                }

                i++;
            }

            if (alreadExist == null)
            {
                var eventLogEntry = new IntegrationEventLogEntry(@event);
                eventLogEntry.State = EventState.ProcessCompleted.ToString();

                _integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);

                await _integrationEventLogContext.SaveChangesAsync();
            }
            else
            {
                alreadExist.State = EventState.ProcessCompleted.ToString();

                var policy = Policy.Handle<Exception>()
                .WaitAndRetry(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.Log(LogLevel.Error, $"SaveEventProcessComplete: Retry update event {@event}," +
                        $" ex:{ex.Message}");
                });

                await policy.Execute(() =>
                {
                    _integrationEventLogContext.Update(alreadExist);
                    return _integrationEventLogContext.SaveChangesAsync();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task SaveEventProcessComplete(IntegrationEvent @event, IDbContextTransaction transaction)
        {
            var alreadExist = await CheckEventExist(@event.Id.ToString());
            int i = 1;

            while (alreadExist == null)
            {
                alreadExist = await CheckEventExist(@event.Id.ToString());

                if (i > 5)
                {
                    _logger.Log(LogLevel.Error, $"SaveEventProcessComplete: Not fount event {@event.Id}," +
                        $" content: {alreadExist.Content}");
                    throw new FriendlyException("Not found event to update");
                }

                i++;
            }

            if (alreadExist == null)
            {
                var eventLogEntry = new IntegrationEventLogEntry(@event);
                eventLogEntry.State = EventState.ProcessCompleted.ToString();

                _integrationEventLogContext.Database.UseTransaction(transaction.GetDbTransaction());
                _integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);

                await _integrationEventLogContext.SaveChangesAsync();
            }
            else
            {
                alreadExist.State = EventState.ProcessCompleted.ToString();

                var policy = Policy.Handle<Exception>()
                .WaitAndRetry(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.Log(LogLevel.Error, $"SaveEventProcessComplete: Retry update event {@event.Id}," +
                        $" content: {alreadExist.Content}, ex:{ex.Message}");
                    _integrationEventLogContext.Database.CurrentTransaction.Dispose();
                });

                await policy.Execute(() =>
                {
                    _integrationEventLogContext.Database.UseTransaction(transaction.GetDbTransaction());
                    _integrationEventLogContext.Update(alreadExist);
                    return _integrationEventLogContext.SaveChangesAsync();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task SaveEventProcessFailed(IntegrationEvent @event)
        {
            var alreadExist = await CheckEventExist(@event.Id.ToString());
            int i = 1;

            while (alreadExist == null)
            {
                alreadExist = await CheckEventExist(@event.Id.ToString());

                if (i > 5)
                {
                    _logger.Log(LogLevel.Error, $"SaveEventProcessFailed: Not fount event {@event.Id}");
                    throw new FriendlyException("Not found event to update");
                }

                i++;
            }

            if (alreadExist == null)
            {
                var eventLogEntry = new IntegrationEventLogEntry(@event);
                eventLogEntry.State = EventState.ProcessFailed.ToString();

                _integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);

                await _integrationEventLogContext.SaveChangesAsync();
            }
            else
            {
                alreadExist.State = EventState.ProcessFailed.ToString();
                alreadExist.TimesSent++;

                var policy = Policy.Handle<Exception>()
                .WaitAndRetry(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.Log(LogLevel.Error, $"SaveEventProcessFailed: Retry update event {@event.Id}, content: {alreadExist}," +
                        $" ex: {ex.Message}");
                });

                await policy.Execute(() =>
                {
                    _integrationEventLogContext.Update(alreadExist);
                    return _integrationEventLogContext.SaveChangesAsync();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private Task UpdateEventStatus(string eventId, EventState status)
        {
            var eventLogEntry = _integrationEventLogContext.IntegrationEventLogs.FirstOrDefault(ie => ie.EventId == eventId.ToString());

            // retry get event to update
            if (eventLogEntry == null)
            {
                int i = 1;

                while (i <= 5)
                {
                    eventLogEntry = _integrationEventLogContext.IntegrationEventLogs.FirstOrDefault(ie => ie.EventId == eventId.ToString());

                    if (i > 5)
                    {
                        _logger.Log(LogLevel.Error, $"Not fount event: {eventLogEntry}");
                        throw new FriendlyException("Not found event to update");
                    }

                    i++;
                }
            }

            eventLogEntry.State = status.ToString();

            if (status == EventState.InProgress)
                eventLogEntry.TimesSent++;

            var policy = Policy.Handle<Exception>()
            .WaitAndRetry(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
            {
                _logger.Log(LogLevel.Error, $"Retry update event status:{status}, event: {eventLogEntry}");
            });

            return policy.Execute(() =>
            {
                _integrationEventLogContext.IntegrationEventLogs.Update(eventLogEntry);
                return _integrationEventLogContext.SaveChangesAsync();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<IntegrationEventLogEntry> CheckEventExist(string eventId)
        {
            try
            {

                var integrationEventLog = await _integrationEventLogContext.IntegrationEventLogs.AsNoTracking().FirstOrDefaultAsync(x =>
                x.EventId == eventId);
                return integrationEventLog;
            }
            catch (Exception ex)
            {
                string message = $":bangbang: *[{_serviceName}] Check Event Exist Fail*\n" +
                                 $"\t*EventId*: {eventId}\n" +
                                 $"\t*Error*: Exception: _{ex.Message}_; InnerException: _{ex.InnerException?.Message}_";
                await _warningService.SendSlackWarning(message);

                throw new EventException($"CheckEventExist, eventId: {eventId}, ex:{ex.Message}");
            }
        }

        /// <summary>
        /// Check Event Exist
        /// </summary>
        /// <param name="@event"></param>
        /// <returns></returns>
        public async Task<IntegrationEventLogEntry> CheckEventExist(IntegrationEvent @event)
        {
            try
            {

                string eventId = @event.Id.ToString();
                var integrationEventLog = await _integrationEventLogContext.IntegrationEventLogs.AsNoTracking()
                                                .FirstOrDefaultAsync(x => x.EventId == eventId);
                return integrationEventLog;
            }
            catch (Exception ex)
            {
                throw new EventException($"Check Event Exist Fail, ex:{ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task SaveEventProcessing(IntegrationEvent @event)
        {
            string eventName = null;
            string content = null;

            try
            {
                var eventLogEntry = new IntegrationEventLogEntry(@event);
                eventLogEntry.State = EventState.Processing.ToString();

                // get content for send alert
                eventName = eventLogEntry.EventTypeShortName;
                content = eventLogEntry.Content;

                var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.Log($"SaveEventProcessing: Retry update event: {eventName}, Content:{content}");
                });

                await policy.ExecuteAsync(async () =>
                {
                    _integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);
                    await _integrationEventLogContext.SaveChangesAsync();
                    _integrationEventLogContext.Entry(eventLogEntry).State = EntityState.Detached;
                });
            }
            catch (Exception ex)
            {
                throw new EventException($"Save Event Processing, ex:{ex.Message}");
            }
        }

        /// <summary>
        /// Update event processing
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task UpdateEventProcessing(IntegrationEvent @event)
        {
            string eventName = null;
            string content = null;

            try
            {
                var eventLogEntry = new IntegrationEventLogEntry(@event);
                eventLogEntry.State = EventState.Processing.ToString();

                // get content for send alert
                eventName = eventLogEntry.EventTypeShortName;
                content = eventLogEntry.Content;

                var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.Log(LogLevel.Error, $"Retry update event processing: {@event.Id}, eventName: {eventName}");
                });

                await policy.ExecuteAsync(async () =>
                {
                    _integrationEventLogContext.IntegrationEventLogs.Update(eventLogEntry);
                    await _integrationEventLogContext.SaveChangesAsync();
                    _integrationEventLogContext.Entry(eventLogEntry).State = EntityState.Detached;
                });
            }
            catch (Exception ex)
            {
                throw new EventException($"Update Event Processing, ex:{ex.Message}");
            }
        }
    }
}
