using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.DAL.Model.Tenant
{
    public class TenantInfo
    {
        public string Id { get; set; }

        public string TenancyName { get; set; }

        public string ConnectionString { get; set; }

        public MultiTenantType MultiTenantType { get; set; }

        public TenantInfo(string id, string tenancyName, string connectionString, MultiTenantType multiTenantType)
        {
            Id = id;
            TenancyName = tenancyName;
            ConnectionString = connectionString;
            MultiTenantType = multiTenantType;
        }

        public TenantInfo(string id, string tenancyName, string connectionString)
        {
            Id = id;
            TenancyName = tenancyName;
            ConnectionString = connectionString;
        }
    }
}