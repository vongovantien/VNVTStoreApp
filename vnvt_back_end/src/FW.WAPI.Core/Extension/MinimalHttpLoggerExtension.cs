using FW.WAPI.Core.General;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Extension
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseMinimalHttpLogger(this IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, ReplaceLoggingHttpMessageHandlerBuilderFilter>());
            return services;
        }
    }

    internal class ReplaceLoggingHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;

        public ReplaceLoggingHttpMessageHandlerBuilderFilter(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;
            _configuration = configuration;
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return builder =>
            {
                next(builder);

                var loggerName = !string.IsNullOrEmpty(builder.Name) ? builder.Name : "Default";
                var innerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.ClientHandler");
                var toRemove = builder.AdditionalHandlers.Where(h => (h is LoggingHttpMessageHandler) || h is LoggingScopeHttpMessageHandler).Select(h => h).ToList();
                foreach (var delegatingHandler in toRemove)
                {
                    builder.AdditionalHandlers.Remove(delegatingHandler);
                }
                builder.AdditionalHandlers.Add(new RequestEndOnlyLogger(innerLogger, _configuration));
            };
        }
    }

    internal class RequestEndOnlyLogger : DelegatingHandler
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public RequestEndOnlyLogger(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            var requestUri = request.RequestUri.ToString(); //SendAsync modifies req uri in case of redirects (?!), so making a local copy
            var stopwatch = ValueStopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                string serviceName = _configuration.GetSection("ServiceName").Value;
                string message = $"{request.Method} {requestUri} - {(int)response.StatusCode} {response.StatusCode} in {stopwatch.GetElapsedTime().TotalMilliseconds}ms";
                var logSouce = new { ServiceName = serviceName, Message = message };
                _logger.LogInformation(JsonUtilities.ConvertObjectToJson(logSouce));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Log http request failed");
            }

            return response;
        }

        internal struct ValueStopwatch
        {
            private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

            private long _startTimestamp;

            public bool IsActive => _startTimestamp != 0;

            private ValueStopwatch(long startTimestamp)
            {
                _startTimestamp = startTimestamp;
            }

            public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

            public TimeSpan GetElapsedTime()
            {
                if (!IsActive)
                {
                    throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");
                }

                long end = Stopwatch.GetTimestamp();
                long timestampDelta = end - _startTimestamp;
                long ticks = (long)(TimestampToTicks * timestampDelta);
                return new TimeSpan(ticks);
            }
        }
    }
}