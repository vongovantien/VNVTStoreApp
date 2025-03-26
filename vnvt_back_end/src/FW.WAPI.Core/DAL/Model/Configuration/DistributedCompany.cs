using System.Collections.Generic;

namespace FW.WAPI.Core.DAL.Model.Configuration
{
    public class DistributedCompany
    {
        public string TableCompany { get; set; }
        public string ParentColumnName { get; set; }
        public List<DistributedConfig> DistributedConfigs { get; set; }
    }

    public class DistributedConfig
    {
        public string TableName { get; set; }
        public string TableDistributedCompany { get; set; }
        public string RefColItemName { get; set; }
        public string RefCompanyColName { get; set; }
        public DistributedType DistributedType { get; set; }
    }

    public enum DistributedType
    {
        Single,
        Multiple
    }
}