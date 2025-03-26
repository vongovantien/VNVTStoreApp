using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.Configuration
{
    public interface IStartupCoreOptions
    {
        string ConnectionString { get; set; }
        bool IsMultyTenancy { get; set; }
        IDictionary<string, string> Tenants { get; set; }
        bool IsAuditing { get; set; }
        LogLevel LogLevel { get; set; }
        DatabaseProvider DatabaseProvider { get; set; }
        Type Tenant { get; set; }
        string EventLogTableName { get; set; }
        string EventLogPKName { get; set; }
        Type SystemItemListType { get; set; }
        string TenantCode { get; set; }
    }
}