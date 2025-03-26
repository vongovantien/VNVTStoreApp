using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.Runtime.Session
{
    public interface IBaseSession
    {
        //string GetTenantCode();

        string UserName { get; }
        string TenantCode { get; }
        string TenantRequest { get; }

        MultiTenantType GetMultiTenantType();

        string[] Roles { get; }

        string RoleCode { get; }

        string CompanyCode { get; }

        string[] CompanyCodes { get; }

        /// <summary>
        /// Get Claim Values By Key
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        string[] GetClaimsValueByKey(params string[] keys);

        /// <summary>
        /// Get Tenant Registered
        /// </summary>
        string TenantRegistered { get; }
    }
}