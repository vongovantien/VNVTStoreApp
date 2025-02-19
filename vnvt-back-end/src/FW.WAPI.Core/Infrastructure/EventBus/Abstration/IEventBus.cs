using FW.WAPI.Core.Infrastructure.EventBus.Event;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Infrastructure.EventBus.Abstration
{
    public interface IEventBus
    {
        bool Publish(IntegrationEvent @event);
        bool Publish(string exchangeName, IntegrationEvent @event);
        void Subscribe<T, TH>(string queueName = null)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void SubscribeDynamic<TH>(string eventName, string queueName = null)
            where TH : IDynamicIntegrationEventHandler;

        void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;

        void AddSubcription<T, TH>() where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;

        Task ProcessEvent(string eventName, string message);

        /// <summary>
        /// Publish To Topic Exchange
        /// </summary>
        /// <param name="event"></param>
        /// <param name="biddingKey"></param>
        /// <param name="exchangeName"></param>
        void PublishToTopicExchange(IntegrationEvent @event, string biddingKey, string exchangeName);

        /// <summary>
        /// Start Basic Consume All Queue
        /// </summary>
        /// <exception cref="WarningException"></exception>
        void StartBasicConsumeAllQueue();
    }
}
