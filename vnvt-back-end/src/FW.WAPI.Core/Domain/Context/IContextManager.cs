using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Domain.Context
{
    public interface IContextManager
    {
        Task<bool> CreateTenantDb<TenantContext>(string connectionString) where TenantContext : DbContext;
        TenantContext GetTenantContext<TenantContext>(string connectionString) where TenantContext : DbContext;
    }
}
