### Task: Implement Resilience Patterns for Cross-Service Calls

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: Cross-service communication over a network is inherently unreliable. The `VehicleServiceClient` currently has no resilience mechanisms and will fail permanently if the `Vehicle.Service` is temporarily unavailable or slow. This task involves implementing resilience patterns like Retry and Circuit Breaker using the [Polly](https://github.com/App-vNext/Polly) library to make the `Insurance.Service` more robust.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Program.cs`
    -   `src/Insurance/Insurance.Service/Insurance.Service.csproj`
    -   `src/Insurance/Insurance.UnitTests/` (for new tests)

-   **Action Points**:

    1.  **Add Polly NuGet Package**: In `src/Insurance/Insurance.Service/Insurance.Service.csproj`, add a reference to the `Microsoft.Extensions.Http.Polly` NuGet package. This package provides seamless integration between `HttpClientFactory` and Polly.
    2.  **Define Retry Policy**: In `Program.cs`, when registering the `VehicleServiceClient`, define a retry policy. This policy should handle transient HTTP failures (like `5xx` errors, `408` timeouts, or `HttpRequestException`). A good starting point is an exponential backoff policy (e.g., wait 1s, then 2s, then 4s before failing).
        ```csharp
        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(4)
        }));
        ```
    3.  **Define Circuit Breaker Policy**: Chain a circuit breaker policy after the retry policy. The circuit breaker will "open" after a certain number of consecutive failures, causing subsequent calls to fail immediately without waiting for a timeout. This prevents the calling service from wasting resources on a known-to-be-unhealthy dependency.
        ```csharp
        .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)
        ));
        ```
    4.  **Apply Policies to HttpClient**: In `Program.cs`, apply these policies to the `HttpClient` registration for the `IVehicleServiceClient`. The final registration might look like this:
        ```csharp
        builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>()
            .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(...))
            .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(...));
        ```
    5.  **Add Timeout Policy**: It's also good practice to add an overall timeout policy to prevent requests from waiting indefinitely. This can be done with `Polly.Timeout.TimeoutPolicy`.
    6.  **Add Unit Tests**: In `Insurance.UnitTests`, add new tests to verify the resilience policies are in place. This is complex to unit test directly, but you can test the fallback behavior of the service when the client signals a failure (e.g., by throwing `BrokenCircuitException`). A better approach is to use integration tests with a mock server (as mentioned in the negative-path testing task) to verify the retry and circuit breaker behaviors.

-   **Verification**:
    -   Run the application and use a tool like `mitmproxy` or `Fiddler` to intercept and drop or delay requests to `Vehicle.Service`. Observe the logs of `Insurance.Service` to confirm that the retry policy is being triggered.
    -   Continuously fail requests to `Vehicle.Service` and verify that the circuit breaker opens (subsequent requests fail instantly) and then transitions to half-open and closed states as expected.
