using FW.WAPI.Core.ExceptionHandling;
using FW.WAPI.Core.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Extension
{
    /// <summary>
    ///
    /// </summary>
    public class DomainResolverMiddleware
    {
        private readonly RequestDelegate _next;

        public DomainResolverMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="tenantResolver"></param>
        /// <param name="tenantStore"></param>
        /// <param name="startupCoreOptions"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext, ITenantStore tenantStore)
        {
            try
            {
                await tenantStore.RegisterTenant();
                
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                httpContext.Response.ContentType = "application/json";

                if (ex is TenantNotFoundException)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { Message = "Not found tenant" }));
                }
                else
                {
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { Message = "Handle request error." }));
                }
            }

        }
    }
}