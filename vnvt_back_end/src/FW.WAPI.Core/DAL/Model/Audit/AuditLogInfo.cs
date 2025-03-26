using System;

namespace FW.WAPI.Core.DAL.Model.Audit
{
    public class AuditLogInfo
    {
        public long Code { get; set; }
        public string BrowserInfo { get; set; }
        public string ClientIpAddress { get; set; }
        public string ClientName { get; set; }
        public string Exception { get; set; }
        public int? ExecutionDuration { get; set; }
        public DateTime? ExecutionTime { get; set; }
        public string MethodName { get; set; }
        public string Parameters { get; set; }
        public string ControllerName { get; set; }
        public string TenantId { get; set; } 
        public string UserId { get; set; }
        public string ServiceName { get; set; }
        public int? ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
}