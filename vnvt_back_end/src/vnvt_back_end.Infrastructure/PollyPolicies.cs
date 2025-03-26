using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vnvt_back_end.Infrastructure
{
    public static class PollyPolicies
    {
        public static RetryPolicy RetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetry(
                retryCount: 3, // Number of retry attempts
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential back-off
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    // Log or handle the retry attempt
                    Console.WriteLine($"Retry {retryCount} encountered an error: {exception.Message}. Waiting {timeSpan} before next retry.");
                });
    }
}
