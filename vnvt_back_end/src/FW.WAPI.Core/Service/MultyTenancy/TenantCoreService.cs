using FW.WAPI.Core.DAL.Model.Tenant;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FW.WAPI.Core.Service.MultyTenancy
{
    public class TenantCoreService<HostDBContext, Tenant> : ITenantCoreService<Tenant>
        where HostDBContext : DbContext
        where Tenant : class, IBaseTenant
    {
        private readonly HostDBContext _hostDBContext;
        //private readonly string _tableName;

        public TenantCoreService(HostDBContext hostDBContext)
        {
            _hostDBContext = hostDBContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantName"></param>
        /// <returns></returns>
        public async Task<Tenant> GetTenantByName(string tenantName)
        {
            return await _hostDBContext.Set<Tenant>().AsNoTracking().FirstOrDefaultAsync(x => x.Name == tenantName && x.IsActive);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public Task<Tenant> GetTenantByCode(string tenantId)
        {
            return _hostDBContext.Set<Tenant>().AsNoTracking().FirstOrDefaultAsync(x => x.Code == tenantId && x.IsActive);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Tenant>> GetAll()
        {
            return await _hostDBContext.Set<Tenant>().AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public Tenant GetTenant(string tenantCode)
        {
            return _hostDBContext.Set<Tenant>().AsNoTracking().FirstOrDefault(x =>
            x.Code == tenantCode && x.IsActive);
        }

        /// <summary>
        /// Get All Tenant Active
        /// </summary>
        /// <returns></returns>
        public async Task<List<Tenant>> GetAllActive()
        {
            return await _hostDBContext.Set<Tenant>().AsNoTracking().Where(x=>x.IsActive).ToListAsync();
        }
    }
}