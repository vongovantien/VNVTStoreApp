using System.Collections.Generic;
using System.Threading.Tasks;

namespace FW.WAPI.Core.MultiTenancy
{
    public interface ITenantStore
    {
        //TenantInfo FindByCode(string tenantId);

        //TenantInfo FindByName(string tenancyName);

        Task RegisterTenant();

        Task<object> GetTenantAsync(string tenantCode);

        object GetTenant(string tenantCode);

        /// <summary>
        /// Regist Tenant
        /// </summary>
        Task RegisterTenant(string tenantCode);

        /// <summary>
        /// Cache All Tenant Active
        /// </summary>
        /// <returns></returns>
        Task<List<string>> CacheAllTenantActive();
    }
}