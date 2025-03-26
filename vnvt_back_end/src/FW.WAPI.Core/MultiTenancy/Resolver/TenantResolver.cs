
namespace FW.WAPI.Core.MultiTenancy.Resolver
{
    public class TenantResolver : ITenantResolver
    {
        private readonly IDomainTenantResolve _domainTenantResolveContributor;
        private readonly IHttpHeaderTenantResolve _httpHeaderTenantResolve;
        private readonly IHttpCookieTenantResolve _httpCookieTenantResolve;
        
        public TenantResolver(IDomainTenantResolve domainTenantResolveContributor,
            IHttpCookieTenantResolve httpCookieTenantResolve,
            IHttpHeaderTenantResolve httpHeaderTenantResolve)
        {
            _domainTenantResolveContributor = domainTenantResolveContributor;
            _httpCookieTenantResolve = httpCookieTenantResolve;
            _httpHeaderTenantResolve = httpHeaderTenantResolve;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string ResolveTenantId()
        {
            string domain = _domainTenantResolveContributor.ResolveTenantId();

            if (domain == null)
            {
                domain = _httpCookieTenantResolve.ResolveTenantId();
            }

            if (domain == null)
            {
                domain = _httpHeaderTenantResolve.ResolveTenantId();
            }

            return domain;
        }
    }
}