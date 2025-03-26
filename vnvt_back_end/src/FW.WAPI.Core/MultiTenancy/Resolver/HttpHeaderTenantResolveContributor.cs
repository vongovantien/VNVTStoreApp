using FW.WAPI.Core.General;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace FW.WAPI.Core.MultiTenancy.Resolver
{
    partial class HttpHeaderTenantResolveContributor : IHttpHeaderTenantResolve
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpHeaderTenantResolveContributor(IHttpContextAccessor httpContextAccessor)
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

            var tenantIdHeader = httpContext.Request.Headers[MultiTenancyConsts.TenantIdResolveKey];
            if (tenantIdHeader == string.Empty || tenantIdHeader.Count < 1)
            {
                return null;
            }

            if (tenantIdHeader.Count > 1)
            {
                return null;
            }

            return tenantIdHeader.FirstOrDefault();
        }
    }
}