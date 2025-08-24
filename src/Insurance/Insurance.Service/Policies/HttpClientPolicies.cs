using System.Net;
using Polly;
using Polly.Extensions.Http;
using ILogger = Serilog.ILogger;

namespace Insurance.Service.Policies;

public static class HttpClientPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (delegateResult, timeSpan, retryAttempt, _) =>
                {
                    logger.Warning(
                        "Request failed with {StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryAttempt}. Exception: {Exception}",
                        delegateResult.Result?.StatusCode,
                        timeSpan,
                        retryAttempt,
                        delegateResult.Exception?.Message);
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger, int handledEventsAllowedBeforeBreaking = 5)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, timespan, _) => { logger.Error("Circuit breaker tripped for {timespan} seconds due to {exception}.", timespan.TotalSeconds, result.Exception?.Message); },
                onReset: (_) => { logger.Information("Circuit breaker reset."); }
            );
    }

    public static IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy(ILogger logger)
    {
        return Policy<HttpResponseMessage>
            .Handle<Exception>()
            .FallbackAsync(
                _ =>
                {
                    var emptyResponse = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
                    };
                    return Task.FromResult(emptyResponse);
                },
                delegateResult =>
                {
                    logger.Error("An unhandled exception occurred. Triggering fallback. Exception: {exception}", delegateResult.Exception?.Message);
                    return Task.CompletedTask;
                }
            );
    }
}
