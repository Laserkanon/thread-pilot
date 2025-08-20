### Task: Expose Application Metrics for Prometheus

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: To monitor the health and performance of the services, we need to expose key application metrics. This task involves using OpenTelemetry to collect standard metrics (e.g., request duration, request count) and expose them on a `/metrics` endpoint in a format compatible with [Prometheus](https://prometheus.io/).
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Program.cs`
    -   `src/Vehicle/Vehicle.Service/Program.cs`
    -   `src/Insurance/Insurance.Service/Insurance.Service.csproj`
    -   `src/Vehicle/Vehicle.Service/Vehicle.Service.csproj`

-   **Action Points**:

    1.  **Add OpenTelemetry Metrics Packages**: In the `.csproj` files for both services, add the following packages. Some may already exist if the distributed tracing task was completed.
        -   `OpenTelemetry.Exporter.Prometheus.AspNetCore`: The key package that provides the `/metrics` scraping endpoint.
        -   `OpenTelemetry.Extensions.Hosting`: Core OTel package.
        -   `OpenTelemetry.Instrumentation.AspNetCore`: To collect metrics for incoming requests.
        -   `OpenTelemetry.Instrumentation.Http`: To collect metrics for outgoing HttpClient calls.

    2.  **Configure OpenTelemetry Metrics in `Program.cs`**: In the `Program.cs` for both services, extend the `AddOpenTelemetry()` call to configure metrics collection.
        ```csharp
        // In Program.cs, inside the AddOpenTelemetry() call
        .WithMetrics(meterProviderBuilder =>
            meterProviderBuilder
                .ConfigureResource(resource => resource
                    .AddService(serviceName: builder.Environment.ApplicationName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
        ```

    3.  **Map the Prometheus Endpoint**: In `Program.cs` for both services, after `app.UseRouting()`, map the Prometheus scraping endpoint. This endpoint should typically not be protected by authorization.
        ```csharp
        // After app.UseRouting() and before app.UseAuthorization()
        app.MapPrometheusScrapingEndpoint();
        ```
        This will expose the metrics at the default `/metrics` path.

    4.  **Add Custom Metrics (Example)**: The task for adding feature toggle metrics already covered creating custom metrics. This setup will automatically expose those custom metrics as well, demonstrating the power of a unified observability stack. No extra work is needed here if that task is also implemented.

-   **Verification**:
    -   Run the services via `docker-compose up`.
    -   Make a few requests to the API endpoints of both services.
    -   Navigate to `http://localhost:5081/metrics` for the Vehicle service and `http://localhost:5082/metrics` for the Insurance service.
    -   Verify that you see Prometheus-formatted metrics in the response.
    -   Look for standard metrics like `http_server_request_duration_seconds` and `http_client_request_duration_seconds`.
    -   If the custom feature toggle metric was implemented, you should also see `feature_toggle_evaluations_total` in the output of the Insurance service's metrics endpoint.
