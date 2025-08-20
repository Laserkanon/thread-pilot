using System.Net;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using ILogger = Serilog.ILogger;

namespace Insurance.Service.Policies;

public static class HttpClientPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger, int handledEventsAllowedBeforeBreaking = 5)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, timespan, context) => { logger.Error("Circuit breaker tripped for {timespan} seconds due to {exception}.", timespan.TotalSeconds, result.Exception?.Message); },
                onReset: (context) => { logger.Information("Circuit breaker reset."); }
            );
    }

    public static IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy(ILogger logger)
    {
        return Policy<HttpResponseMessage>
            .Handle<BrokenCircuitException>()
            .FallbackAsync(
                fallbackAction: cancellationToken =>
                {
                    logger.Error("Circuit is open. Returning fallback empty response.");
                    var emptyResponse = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
                    };
                    return Task.FromResult(emptyResponse);
                }
            );
    }
}
