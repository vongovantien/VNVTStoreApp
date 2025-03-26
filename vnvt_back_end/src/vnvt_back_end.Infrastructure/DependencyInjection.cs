using Microsoft.Extensions.DependencyInjection;
using vnvt_back_end.Infrastructure.Logging;

namespace vnvt_back_end.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            {
                services.AddSingleton<ILoggerService, LoggerService>();
                return services;

            }
        }
    }
}
