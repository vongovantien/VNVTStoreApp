using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.Configuration
{
    /// <summary>
    ///
    /// </summary>
    public class StartupCoreOptions : IStartupCoreOptions
    {
        public string ConnectionString { get; set; }
        public bool IsMultyTenancy { get; set; }
        public IDictionary<string, string> Tenants { get; set; }
        public bool IsAuditing { get; set; }
        public LogLevel LogLevel { get; set; }
        public DatabaseProvider DatabaseProvider { get; set; }
        public Type SystemItemListType { get; set; }
        public Type Tenant { get; set; }
        public string EventLogTableName { get; set; }
        public string EventLogPKName { get; set; }
        public string TenantCode { get; set; }

        public StartupCoreOptions(bool isMultyTenancy, bool isAuditing, DatabaseProvider databaseProvider,
            Type systemItemListType)
        {
            IsMultyTenancy = isMultyTenancy;
            Tenants = new Dictionary<string, string>();
            IsAuditing = isAuditing;
            DatabaseProvider = databaseProvider;
            SystemItemListType = systemItemListType;
        }
    }
}