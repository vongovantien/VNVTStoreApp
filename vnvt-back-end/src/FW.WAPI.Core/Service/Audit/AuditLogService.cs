using FW.WAPI.Core.Repository;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Service.Audit
{
    public class AuditLogService<TDataContext, TAuditLog> : IAuditLogService<TDataContext, TAuditLog>
        where TAuditLog : class
        where TDataContext : DbContext
    {
        private readonly IRepository<TDataContext, TAuditLog> _auditRepository;

        public AuditLogService(IRepository<TDataContext, TAuditLog> auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task<bool> SaveAudit(TAuditLog auditLog)
        {
            var result = true;
            try
            {
                _auditRepository.Insert(auditLog);
                await _auditRepository.SaveChangesAsync();
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}