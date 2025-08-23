### Task: Refactor Controller to Depend on Service Layer

-   **Priority**: Medium
-   **Complexity**: Low
-   **Description**: The `VehiclesController` currently depends directly on `IVehicleRepository` to fetch data. This breaks the intended architectural pattern of having controllers depend on a service layer, which then coordinates data access via repositories. To improve separation of concerns and align with the architecture of the `Insurance.Service`, the `VehiclesController` should be refactored to use a new `IVehicleService`.
-   **Affected Files**:
    -   `src/Vehicle/Vehicle.Service/Controllers/VehiclesController.cs`
    -   `src/Vehicle/Vehicle.Service/Program.cs`
-   **Action Points**:
    1.  **Create `IVehicleService` Interface**: Define a new `IVehicleService` interface in a `Services` directory. It should expose methods that the controller needs, such as `Task<Models.Vehicle> GetVehicleAsync(string registrationNumber)` and `Task<IEnumerable<Models.Vehicle>> GetVehiclesAsync(string[] registrationNumbers)`.
    2.  **Create `VehicleService` Implementation**: Create a `VehicleService` class that implements `IVehicleService`. This service will take `IVehicleRepository` as a dependency and will contain the logic for fetching vehicle data.
    3.  **Register the Service**: In `Program.cs`, register the new service with the dependency injection container (e.g., `builder.Services.AddScoped<IVehicleService, VehicleService>();`).
    4.  **Refactor `VehiclesController`**: Update the `VehiclesController` to depend on `IVehicleService` instead of `IVehicleRepository`. The controller's methods should now call the service layer to get data.
    5.  **Verification**: Run the application and its tests to ensure that the refactoring was successful and all vehicle-related endpoints still function correctly.
