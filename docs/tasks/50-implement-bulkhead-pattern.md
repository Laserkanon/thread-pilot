### Task: Implement the Bulkhead Resilience Pattern

-   **Priority**: Low
-   **Complexity**: Medium
-   **Description**: To further improve the resilience of the system, the Bulkhead pattern should be implemented for calls between services. This pattern isolates resources (like thread pools) used for different downstream calls, preventing a failure or slowdown in one dependency from consuming all available resources and causing a cascading failure to other, unrelated parts of the system.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Program.cs`
    -   `src/Insurance/Insurance.Service/Policies/HttpClientPolicies.cs`
-   **Action Points**:
    1.  **Create a Bulkhead Policy**: In `HttpClientPolicies.cs`, create a new static method that returns a Polly `BulkheadPolicy`. This policy will limit the number of concurrent executions and the number of queued actions for the `HttpClient` calls.
        ```csharp
        // Example Bulkhead Policy
        public static IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy(ILogger logger)
        {
            return Policy.BulkheadAsync<HttpResponseMessage>(10, 2, onBulkheadRejectedAsync: context =>
            {
                logger.LogWarning("Bulkhead rejected the execution.");
                return Task.CompletedTask;
            });
        }
        ```
    2.  **Apply the Policy**: In `Insurance.Service/Program.cs`, add the new `BulkheadPolicy` to the `HttpClient` configuration for the `VehicleServiceClient`. It should be placed early in the chain, before the Retry or Circuit Breaker policies.
    3.  **Tune Parameters**: The parameters for the bulkhead (max parallelization, max queuing actions) need to be tuned based on expected load and resource constraints. The values in the example are placeholders.
-   **Verification**:
    -   Load testing (as described in another task) is the most effective way to verify the bulkhead. Under high load, you should see the `onBulkheadRejectedAsync` log messages being printed when the concurrency limit is reached.
    -   The service should remain responsive to other requests even when the calls to the `Vehicle.Service` are being throttled by the bulkhead.
