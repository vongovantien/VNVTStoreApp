using System.Threading.Tasks;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Runtime.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FW.WAPI.Core.Filter
{
    public class TenantAuthorizationFilter : IAsyncActionFilter
    {
        private readonly string tenantCodeHeader;
        private readonly IBaseSession _baseSession;

        public TenantAuthorizationFilter(IBaseSession baseSession, IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor.HttpContext.Request.Headers.ContainsKey(TokenConst.TENANT_CODE))
            {
                tenantCodeHeader = httpContextAccessor.HttpContext.Request.Headers[TokenConst.TENANT_CODE];
            }

            _baseSession = baseSession;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (string.IsNullOrEmpty(tenantCodeHeader))
            {
                context.Result = new NotFoundResult();
                return;
            }
            else
            {
                if (_baseSession.TenantCode != tenantCodeHeader)
                {
                    context.Result = new NotFoundResult();
                    return;
                }
            }

            await next();
        }
    }
}