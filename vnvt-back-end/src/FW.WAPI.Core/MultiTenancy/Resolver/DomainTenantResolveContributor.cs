using FW.WAPI.Core.General;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace FW.WAPI.Core.MultiTenancy.Resolver
{
    public class DomainTenantResolveContributor : IDomainTenantResolve
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebMultiTenancyConfiguration _multiTenancyConfiguration;

        public DomainTenantResolveContributor(IHttpContextAccessor httpContextAccessor,
            IWebMultiTenancyConfiguration webMultiTenancyConfiguration)
        {
            _httpContextAccessor = httpContextAccessor;
            _multiTenancyConfiguration = webMultiTenancyConfiguration;
        }

        public string ResolveTenantId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            var hostName = httpContext.Request.Host.Host.RemovePreFix("http://", "https://").RemovePostFix("/");
            var domainFormat = _multiTenancyConfiguration.DomainFormat.RemovePreFix("http://",
                "https://").Split(':')[0].RemovePostFix("/");
            var result = new FormattedStringValueExtracter().Extract(hostName, domainFormat, true, '/');

            if (!result.IsMatch || !result.Matches.Any())
            {
                return null;
            }

            var tenancyName = result.Matches[0].Value;
            if (tenancyName.IsNullOrEmpty())
            {
                return null;
            }

            if (string.Equals(tenancyName, "www", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return tenancyName;
        }
    }
}