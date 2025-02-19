using Microsoft.AspNetCore.Builder;

namespace FW.WAPI.Core.Extension
{
    public static class DomainResolverMiddlewareExtension
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseDomainResolver(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DomainResolverMiddleware>();
        }
    }
}