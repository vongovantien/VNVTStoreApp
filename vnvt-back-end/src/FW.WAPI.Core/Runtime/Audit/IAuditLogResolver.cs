using FW.WAPI.Core.DAL.Model.Audit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Runtime.Audit
{
    public interface IAuditLogResolver
    {
        bool ShouldSaveAudit(MethodInfo methodInfo, bool defaultValue = false);

        AuditLogInfo CreateAuditInfo(Type type, MethodInfo method, object[] arguments);

        AuditLogInfo CreateAuditInfo(Type type, MethodInfo method, IDictionary<string, object> arguments);

        void Save(AuditLogInfo auditInfo);

        Task<bool> SaveAsync(AuditLogInfo auditInfo);
    }
}