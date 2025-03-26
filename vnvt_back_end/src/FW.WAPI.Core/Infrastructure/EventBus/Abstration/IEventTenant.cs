
namespace FW.WAPI.Core.Infrastructure.EventBus.Abstration
{
    public interface IEventTenant
    {
        string TenantCode { get; set; }
    }
}
