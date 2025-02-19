using System.Collections.Generic;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Service.MultyTenancy
{
    public interface ITenantCoreService<Tenant>
    {
        //Task<Tenant> GetTenantByName(string tenantName);

        Task<Tenant> GetTenantByCode(string tenantId);
        Tenant GetTenant(string tenantCode);

        //Task<List<Tenant>>  GetAll();

        /// <summary>
        /// Get All Tenant Active
        /// </summary>
        /// <returns></returns>
        Task<List<Tenant>> GetAllActive();
    }
}