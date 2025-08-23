### Task: Configure HttpClient Properties for Vehicle Service Client

-   **Priority**: Medium
-   **Complexity**: Low
-   **Description**: The `HttpClient` used by the `VehicleServiceClient` does not have an explicit timeout configured. Relying on the default `HttpClient` timeout (which is often very long, e.g., 100 seconds) can lead to poor performance and thread exhaustion if the downstream service is unresponsive. This task is to configure a reasonable default timeout and other relevant properties for the client.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Program.cs`
-   **Action Points**:
    1.  **Analyze Timeout Requirements**: Determine a reasonable timeout for calls to the `Vehicle.Service`. This should be shorter than the Polly retry policy's timeout to work effectively with it. A value like 10-15 seconds is often a good starting point.
    2.  **Configure HttpClient**: In `Insurance.Service/Program.cs`, extend the `AddHttpClient` configuration for `VehicleServiceClient`.
    3.  Use the `.ConfigureHttpClient()` method to set the `Timeout` property on the `HttpClient` instance.
        ```csharp
        builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15); // Example timeout
            })
            // ... existing Polly policies
        ```
    4.  **Consider Other Properties**: Review other `HttpClient` properties that might be relevant, such as setting a `User-Agent` header for better logging and traceability on the downstream service (e.g., `client.DefaultRequestHeaders.Add("User-Agent", "InsuranceService-HttpClient");`).
    5.  **Verification**: After applying the changes, run the integration tests to ensure the client still functions correctly. Manually test the timeout by pointing the client to a service that delays its response for longer than the configured timeout and verify that a `TaskCanceledException` is thrown.
