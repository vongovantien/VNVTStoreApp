using FW.WAPI.Core.Configuration;
using FW.WAPI.Core.General;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.Runtime.Session
{
    /// <summary>
    ///
    /// </summary>
    public class BaseSession : IBaseSession
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IStartupCoreOptions _startupCoreOptions;
        public BaseSession(IHttpContextAccessor httpContextAccessor,
            IStartupCoreOptions startupCoreOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _startupCoreOptions = startupCoreOptions;
        }

        /// <summary>
        /// Get tenant code from jwt
        /// </summary>
        public string TenantRequest
        {
            get
            {
                return _httpContextAccessor.HttpContext == null ? null :
                   _httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == TokenConst.TENANT_CODE).Select(c => c.Value).FirstOrDefault();
            }
        }

        public string[] Roles
        {
            get
            {
                return _httpContextAccessor.HttpContext == null ? null :
                _httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
            }
        }

        public string RoleCode
        {
            get
            {
                return _httpContextAccessor.HttpContext
                    ?.User.Claims.FirstOrDefault(c => c.Type == TokenConst.ROLE_CODE)
                    ?.Value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string UserName
        {
            get
            {
                return _httpContextAccessor.HttpContext == null ? null :
                _httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == TokenConst.USER_NAME).Select(c => c.Value).FirstOrDefault();
            }
        }

        /// <summary>
        /// Get tenant code from header of request
        /// </summary>
        public string TenantCode
        {
            get
            {
                return _httpContextAccessor.HttpContext == null ? null :
                    _httpContextAccessor.HttpContext.Request.Headers[TokenConst.TENANT_CODE].ToString();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string CompanyCode
        {
            get
            {
                return _httpContextAccessor.HttpContext == null ? null :
                _httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == TokenConst.COMPANY_CODE).Select(c => c.Value).FirstOrDefault();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string[] CompanyCodes
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User.Claims.Where(c => c.Type == TokenConst.COMPANY_CODES).Select(c => c.Value).ToArray();
            }
        }

        public MultiTenantType GetMultiTenantType()
        {
            return MultiTenantType.Host;
        }

        /// <summary>
        /// Get Claims Value By Keys
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public string[] GetClaimsValueByKey(params string[] keys)
        {
            return _httpContextAccessor.HttpContext == null ? null :
                  _httpContextAccessor.HttpContext.User.Claims.Where(c => keys.Contains(c.Type)).Select(c => c.Value).ToArray();
        }

        /// <summary>
        /// Get Tenant Registered
        /// </summary>
        public string TenantRegistered
        {
            get
            {
                return _startupCoreOptions.TenantCode;
            }
        }
    }
}