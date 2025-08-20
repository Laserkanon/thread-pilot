### Task: Add Metrics for Feature Toggle Usage

-   **Priority**: Low
-   **Complexity**: Medium
-   **Description**: To improve observability, we need to understand the impact and usage of the `EnableVehicleEnrichment` feature. This task involves adding metrics to track how often the toggle is evaluated. These metrics can then be exposed through a metrics endpoint for monitoring systems like Prometheus, enabling dashboards and alerts based on feature usage.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Services/FeatureToggleService.cs`
    -   `src/Insurance/Insurance.Service/Program.cs`
-   **Action Points**:
    1.  **Introduce a Meter**: In `FeatureToggleService.cs`, create a static `System.Diagnostics.Metrics.Meter` instance (e.g., `private static readonly Meter MyMeter = new("Insurance.Service.FeatureToggles");`).
    2.  **Create a Counter**: In the `FeatureToggleService` constructor, use the meter to create a `Counter<int>` (e.g., `_evaluations = MyMeter.CreateCounter<int>("feature_toggle.evaluations.total", "evaluations", "The number of times a feature toggle has been evaluated.");`).
    3.  **Increment the Counter**: In the `IsVehicleEnrichmentEnabled()` method, increment the counter each time it's called. Use tags to provide dimensions for the metric, such as the toggle's name and its returned value. For example: `_evaluations.Add(1, new KeyValuePair<string, object?>("toggle_name", "EnableVehicleEnrichment"), new KeyValuePair<string, object?>("value", isEnabled));`.
    4.  **Expose Metrics Endpoint**: In `Program.cs`, add the necessary services and middleware to expose a Prometheus-compatible metrics endpoint. This typically involves adding a package like `OpenTelemetry.Exporter.Prometheus.AspNetCore` and then using `app.MapPrometheusScrapingEndpoint();`.
    5.  **Verification**: After implementation, run the service and access the `/metrics` endpoint (or the configured path) to verify that the new `feature_toggle_evaluations_total` metric appears and updates correctly when the feature toggle is evaluated.
