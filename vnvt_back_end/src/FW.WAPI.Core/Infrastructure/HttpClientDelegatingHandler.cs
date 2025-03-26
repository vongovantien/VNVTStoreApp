using FW.WAPI.Core.General;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Infrastructure
{
    public class HttpClientDelegatingHandler : DelegatingHandler
    {
        private const string CorrelationHeader = "X-Correlation-ID";
        private readonly IHttpContextAccessor _httpContextAccesor;
        private readonly IConfiguration _configuration;

        public HttpClientDelegatingHandler(IHttpContextAccessor httpContextAccesor, IConfiguration configuration)
        {
            _httpContextAccesor = httpContextAccesor;
            _configuration = configuration;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authorizationHeader = _httpContextAccesor.HttpContext?.Request.Headers["Authorization"];

            if (authorizationHeader.HasValue && !StringValues.IsNullOrEmpty(authorizationHeader.Value))
            {
                request.Headers.Remove("Authorization");
                request.Headers.Add("Authorization", new List<string>() { authorizationHeader });
            }
            //Add token
            var token = await GetToken();
            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            string tenantCodeHeader = _httpContextAccesor.HttpContext?.Request.Headers[MultiTenancyConsts.TenantIdResolveKey];

            if (!string.IsNullOrEmpty(tenantCodeHeader))
            {
                if (!request.Headers.Contains(MultiTenancyConsts.TenantIdResolveKey))
                {
                    //request.Headers.Remove(MultiTenancyConsts.TenantIdResolveKey);
                    request.Headers.Add(MultiTenancyConsts.TenantIdResolveKey, tenantCodeHeader);
                }
            }

            //Get correlationid from appsetting
            var serviceCorrellationId = _configuration.GetSection("ServiceCorrellationId").Value;
            if (!string.IsNullOrEmpty(serviceCorrellationId))
            {
                //Add Correlation header
                var correlationHeader = _httpContextAccesor.HttpContext?.Request.Headers[CorrelationHeader];
                if (correlationHeader.HasValue && !StringValues.IsNullOrEmpty(correlationHeader.Value))
                {
                    request.Headers.Remove(CorrelationHeader);
                    request.Headers.Add(CorrelationHeader, serviceCorrellationId);
                }
                else
                {
                    request.Headers.Add(CorrelationHeader, serviceCorrellationId);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }

        async Task<string> GetToken()
        {
            try
            {
                if (_httpContextAccesor.HttpContext == null) return null;

                const string ACCESS_TOKEN = "access_token";
                return await _httpContextAccesor.HttpContext.GetTokenAsync(ACCESS_TOKEN);
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }


}
