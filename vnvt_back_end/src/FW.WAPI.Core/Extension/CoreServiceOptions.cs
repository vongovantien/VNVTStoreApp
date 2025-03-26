using Microsoft.Extensions.Logging;
using System;
using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.Extension
{
    public class CoreServiceOptions
    {
        //public bool IsMultiTenancyEnable { get; set; }
        public string DefaultConnectionString { get; set; }
        public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.MSSQL;
        public string KeyEncrypted { get; set; }
        public string Version { get; set; } = "v1";
        public Type SystemItemListType { get; set; }
        public bool isUseEventLog { get; set; } = false;
        public string EventLogTableName { get; set; }
        public string EventLogPKName { get; set; }
        public string[] AllowOrigins { get; set; }
        public bool IsAuditLog { get; set; } = false;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public ConnectionType ConnectionType { get; set; } = ConnectionType.AppConfig;

        public CoreServiceOptions()
        {
        }
    }
}