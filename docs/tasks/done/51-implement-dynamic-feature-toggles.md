### Task: Implement Dynamic Feature Toggles

-   **Priority**: Medium
-   **Complexity**: Low
-   **Description**: The current feature toggle implementation reads values from `IConfiguration` at startup, which means the application must be restarted for any changes to take effect. This task is to refactor the implementation to use `IOptionsMonitor` to allow for dynamic, real-time reloading of feature toggles from `appsettings.json` without a restart.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Services/FeatureToggleService.cs`
    -   `src/Insurance/Insurance.Service/Program.cs`
-   **Action Points**:
    1.  **Define a Settings Class**: Create a new class, `FeatureToggleSettings`, to strongly-type the `FeatureToggles` section of `appsettings.json`.
        ```csharp
        public class FeatureToggleSettings
        {
            public bool EnableVehicleEnrichment { get; set; }
        }
        ```
    2.  **Configure `IOptions`**: In `Program.cs`, bind the `FeatureToggles` configuration section to the new `FeatureToggleSettings` class using `builder.Services.Configure<FeatureToggleSettings>(...)`.
    3.  **Refactor `FeatureToggleService`**: Update the `FeatureToggleService` to inject `IOptionsMonitor<FeatureToggleSettings>` instead of `IConfiguration`.
    4.  **Update Implementation**: In the `IsVehicleEnrichmentEnabled` method, get the current value from `_optionsMonitor.CurrentValue.EnableVehicleEnrichment`. `IOptionsMonitor` will automatically provide the most up-to-date value if the underlying `appsettings.json` file changes.
-   **Verification**:
    -   Run the `Insurance.Service` locally.
    -   Make a request that triggers the feature toggle and verify the initial behavior.
    -   While the service is still running, change the value of `EnableVehicleEnrichment` in `src/Insurance/Insurance.Service/appsettings.json` and save the file.
    -   Make the same request again and verify that the service's behavior changes immediately, reflecting the new value of the toggle without a restart.
