namespace FW.WAPI.Core.MultiTenancy.Resolver
{
    public interface ITenantResolver
    {
        string ResolveTenantId();
    }
}