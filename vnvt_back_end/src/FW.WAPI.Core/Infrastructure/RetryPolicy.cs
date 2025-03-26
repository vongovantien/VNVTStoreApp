using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;


namespace FW.WAPI.Core.Infrastructure
{
    public static class RetryPolicy
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
        {
            return HttpPolicyExtensions
              .HandleTransientHttpError()
              .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
              .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
