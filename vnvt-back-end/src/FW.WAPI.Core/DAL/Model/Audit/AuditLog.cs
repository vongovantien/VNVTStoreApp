using System;

namespace FW.WAPI.Core.DAL.Model.Audit
{
    public class AuditLog
    {
        public string FunctionCode { get; set; }
        public string UserCode { get; set; }
        public int ActionCode { get; set; }
        public string Parameters { get; set; }
        public DateTime ReceivedTime { get; set; }
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string Description { get; set; }
        public string TenantCode { get; set; }
    }
}