### Task: Implement Vehicle Client Feature Toggle and Resilience

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: The `Insurance.Service` needs to be resilient when calling the `Vehicle.Service`. A feature toggle is required to switch between an efficient batch-retrieval endpoint and individual, concurrent calls. This allows for operational flexibility if the legacy `Vehicle.Service` has issues with batching. The single-call method must be resilient to partial failures and not overwhelm the downstream service.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Clients/IVehicleServiceClient.cs`
    -   `src/Insurance/Insurance.Service/Clients/VehicleServiceClient.cs`
    -   `src/Insurance/Insurance.Service/Services/FeatureToggles.cs`
    -   `src/Insurance/Insurance.Service/Services/IFeatureToggleService.cs`
    -   `src/Insurance/Insurance.Service/Services/FeatureToggleService.cs`
    -   `src/Insurance/Insurance.Service/Policies/HttpClientPolicies.cs`
    -   `src/Insurance/Insurance.Service/appsettings.json`
    -   `src/Insurance/Insurance.UnitTests/Clients/VehicleServiceClientTests.cs`
    -   `README.md`
-   **Action Points**:
    1.  **Rename Batch Method**: Rename `GetVehiclesAsync` to `GetVehiclesBatchAsync` in the `IVehicleServiceClient` and its implementation to make its purpose clear.
    2.  **Add Feature Toggle**: Introduce a new feature toggle `EnableBatchVehicleCall` in `FeatureToggles.cs` and the corresponding service and interface.
    3.  **Implement Toggle Logic**: Create a new `GetVehiclesAsync` method that acts as a dispatcher.
        - If the toggle is on, it calls `GetVehiclesBatchAsync`.
        - If the toggle is off, it uses `Parallel.ForEachAsync` to call a new private `GetVehicleAsync(regNumber)` method for each registration number.
    4.  **Add Concurrency Control**: Add a `MaxDegreeOfParallelism` configuration setting in `appsettings.json` and use it to limit the concurrency of the parallel loop.
    5.  **Add Unit Tests**: Create a comprehensive suite of unit tests covering both the batch and single-call modes. This includes tests for success, not found, partial failure, retry policies, and circuit-breaker/fallback policies.
    6.  **Correct Test Setup**: Refactor the unit tests to use `ServiceCollection` to build a proper dependency injection container. This ensures that the Polly policies are tested using the same `PolicyHttpMessageHandler` that the real application uses, which was a critical step to correctly verify the fallback behavior.
    7.  **Update Documentation**: Update the `README.md` to document the new data enrichment strategy, explaining the rationale behind the feature toggle and concurrency control.
