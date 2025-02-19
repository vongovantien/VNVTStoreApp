using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Service.Audit
{
    public interface IAuditLogService<TDataContext, TAuditLog>
          where TAuditLog : class
        where TDataContext : DbContext
    {
        Task<bool> SaveAudit(TAuditLog auditLog);
    }
}