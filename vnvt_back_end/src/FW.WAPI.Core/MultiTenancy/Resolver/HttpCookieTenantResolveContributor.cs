using FW.WAPI.Core.General;
using Microsoft.AspNetCore.Http;

namespace FW.WAPI.Core.MultiTenancy.Resolver
{
    public class HttpCookieTenantResolveContributor : IHttpCookieTenantResolve
    {
        //TODO: add tenantid cookie in config
        //private const string TenantIdResolveKey = "SM.TenantId";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpCookieTenantResolveContributor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string ResolveTenantId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            var tenantIdValue = httpContext.Request.Cookies[MultiTenancyConsts.TenantIdResolveKey];
            if (tenantIdValue.IsNullOrEmpty())
            {
                return null;
            }

            return tenantIdValue;
        }
    }
}