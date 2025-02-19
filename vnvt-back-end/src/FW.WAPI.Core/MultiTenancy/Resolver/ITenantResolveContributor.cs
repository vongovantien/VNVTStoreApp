namespace FW.WAPI.Core.MultiTenancy.Resolver
{
    public interface ITenantResolveContributor
    {
        string ResolveTenantId();
    }
}