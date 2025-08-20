### Task: Add Distributed Tracing with OpenTelemetry

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: In a microservice architecture, it's critical to be able to trace a single request as it flows through multiple services. This task involves integrating OpenTelemetry (OTel) to add distributed tracing. This will allow us to visualize the entire lifecycle of a request, from the initial API call to `Insurance.Service` to the subsequent downstream call to `Vehicle.Service`, making it much easier to diagnose latency and errors.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Program.cs`
    -   `src/Vehicle/Vehicle.Service/Program.cs`
    -   `src/Insurance/Insurance.Service/Insurance.Service.csproj`
    -   `src/Vehicle/Vehicle.Service/Vehicle.Service.csproj`

-   **Action Points**:

    1.  **Add OpenTelemetry NuGet Packages**: In the `.csproj` files for both `Insurance.Service` and `Vehicle.Service`, add the following OpenTelemetry packages:
        -   `OpenTelemetry.Extensions.Hosting`: The main integration package.
        -   `OpenTelemetry.Instrumentation.AspNetCore`: To automatically trace incoming ASP.NET Core requests.
        -   `OpenTelemetry.Instrumentation.Http`: To automatically trace outgoing HttpClient requests.
        -   `OpenTelemetry.Exporter.Console`: To export traces to the console for easy local debugging.

    2.  **Configure OpenTelemetry in `Program.cs`**: In the `Program.cs` for both services, configure the OpenTelemetry tracer provider.
        ```csharp
        // In Program.cs
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
                tracerProviderBuilder
                    .AddSource(builder.Environment.ApplicationName)
                    .ConfigureResource(resource => resource
                        .AddService(serviceName: builder.Environment.ApplicationName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter()); // Exports to console for local dev
        ```
        This configuration sets up a "Tracer Provider" that:
        -   Identifies the service (`Insurance.Service` or `Vehicle.Service`).
        -   Adds instrumentation to automatically create traces for web requests and HTTP client calls.
        -   Exports the created traces to the console.

    3.  **(Optional) Add Custom Spans**: To add more detail to the traces, you can inject an `ActivitySource` into your services and create custom spans (called Activities in OTel for .NET) around specific operations, like a database call.
        ```csharp
        // In a service class
        private static readonly ActivitySource MyActivitySource = new("MyApplication.MyComponent");

        public async Task MyMethod()
        {
            using (var activity = MyActivitySource.StartActivity("Doing something important"))
            {
                // ... do work ...
                activity?.SetTag("my.tag", "my.value");
            }
        }
        ```

-   **Verification**:
    -   Run the services via `docker-compose up`.
    -   Make a request to the `Insurance.Service` endpoint (e.g., `GET /api/v1/insurances/199001011234`).
    -   Observe the console logs for both services. You should see the trace data printed by the `ConsoleExporter`.
    -   Verify that the output shows a parent span for the request to `Insurance.Service` and a child span for the downstream `HttpClient` call to `Vehicle.Service`.
    -   Crucially, verify that the `TraceId` is the same for both spans across both services, and that the `SpanId` of the `Insurance.Service`'s client call matches the `ParentId` of the `Vehicle.Service`'s server span. This confirms the trace context was propagated correctly.
