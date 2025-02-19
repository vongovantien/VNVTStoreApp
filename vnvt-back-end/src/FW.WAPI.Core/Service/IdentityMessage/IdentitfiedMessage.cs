using FW.WAPI.Core.Infrastructure.EventBus.Event;
using MediatR;


namespace FW.WAPI.Core.Service.IdentityMessage
{
    public class IdentifiedMessage<T, R> : IRequest<R>
    {
        public T Command { get; }
        public IntegrationEvent IntegrationEvent { get; }

        public IdentifiedMessage(T command, IntegrationEvent integrationEvent)
        {
            Command = command;
            IntegrationEvent = integrationEvent;
        }
    }
}
