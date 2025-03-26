using System.Threading.Tasks;

namespace FW.WAPI.Core.Infrastructure.EventBus.Abstration
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}
