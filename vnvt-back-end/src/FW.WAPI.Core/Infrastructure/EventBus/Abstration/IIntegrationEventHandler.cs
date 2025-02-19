using FW.WAPI.Core.Infrastructure.EventBus.Event;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Infrastructure.EventBus.Abstration
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
       where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }

    public interface IIntegrationEventHandler
    {
    }
}
