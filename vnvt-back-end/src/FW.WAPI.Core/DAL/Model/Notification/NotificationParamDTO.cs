using System;
using System.Collections.Generic;
using System.Text;

namespace FW.WAPI.Core.DAL.DTO
{
    public class NotificationParamDTO
    {
        public string Code { get; set; }
        public string CompanyCode { get; set; }
        public string ReferenceCode { get; set; }
        public short Type { get; set; }
        public short Method { get; set; }
        public string PhoneNumber { get; set; }
        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime? ReferenceTime { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerTypeCode { get; set; }
        public string TenantCode { get; set; }
        public string DriverCode { get; set; }
        public bool IsEncrypt { get; set; }
        public bool IsSkipLog { get; set; }

        public NotificationParamDTO() { }

        public NotificationParamDTO(short method, short type,
            string companyCode, string message, string tenantCode)
        {
            Code = Guid.NewGuid().ToString();
            CompanyCode = companyCode;
            Method = method;
            Type = type;
            Message = message;
            TenantCode = tenantCode;
        }
    }
}
