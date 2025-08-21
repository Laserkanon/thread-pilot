### Task: Refactor Feature Toggles for Dynamic Reloading

-   **Priority**: Medium
-   **Complexity**: Low
-   **Description**: The current feature toggle implementation reads from `appsettings.json` only at startup. This prevents dynamic configuration changes without a service restart. By refactoring to use `IOptionsMonitor<T>`, the `Insurance.Service` can reload toggle configurations on the fly, which is essential for operational agility and enabling/disabling features in a production environment without downtime.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Services/FeatureToggleService.cs`
    -   `src/Insurance/Insurance.Service/Services/IFeatureToggleService.cs`
    -   `src/Insurance/Insurance.Service/Program.cs`
    -   `src/Insurance/Insurance.Service/appsettings.json`
-   **Action Points**:
    1.  **Create Options Class**: Create a new class `FeatureTogglesOptions` in a new file under `src/Insurance/Insurance.Service/Models/` to strongly type the `FeatureToggles` configuration section. It should have a property `public bool EnableVehicleEnrichment { get; set; }`.
    2.  **Configure Options in DI**: In `src/Insurance/Insurance.Service/Program.cs`, register the options class with the dependency injection container. Use `builder.Services.Configure<FeatureTogglesOptions>(builder.Configuration.GetSection("FeatureToggles"));` to bind the class to the `appsettings.json` section.
    3.  **Update Service Implementation**: In `src/Insurance/Insurance.Service/Services/FeatureToggleService.cs`, inject `IOptionsMonitor<FeatureTogglesOptions>` into the constructor instead of `IConfiguration`.
    4.  **Update Toggle Logic**: Modify the `IsVehicleEnrichmentEnabled()` method in `FeatureToggleService.cs` to get the value from the options monitor: `return _optionsMonitor.CurrentValue.EnableVehicleEnrichment;`.
    5.  **Verification**: After implementation, manually verify that changing the `EnableVehicleEnrichment` value in `appsettings.json` while the service is running is reflected in the service's behavior without a restart.
