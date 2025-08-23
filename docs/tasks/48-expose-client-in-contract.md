### Task: Expose API Client in Contract Package

-   **Priority**: Medium
-   **Complexity**: Medium
-   **Description**: To make consuming services easier, more consistent, and less error-prone, the client logic for a service should be provided within its own contract package. Currently, any service wanting to consume `Vehicle.Service` must write its own `HttpClient` logic. This task is to move the client interface and registration logic into the `Vehicle.Service.Contracts` package itself.
-   **Affected Files**:
    -   `src/Vehicle/Vehicle.Service.Contracts/` (new files will be added)
    -   `src/Insurance/Insurance.Service/Program.cs` (will be simplified)
    -   `src/Insurance/Insurance.Service/Clients/` (will be removed/replaced)
-   **Action Points**:
    1.  **Define Client Interface**: In the `Vehicle.Service.Contracts` project, define a new public interface, `IVehicleApiClient`, that describes the methods for calling the Vehicle API (e.g., `Task<Vehicle> GetVehicleAsync(string registrationNumber)`).
    2.  **Move Client Implementation**: Move the existing `VehicleServiceClient.cs` implementation from `Insurance.Service` into the `Vehicle.Service.Contracts` project. The implementation class itself can be made `internal` so that consumers only depend on the interface.
    3.  **Create DI Extension Method**: In `Vehicle.Service.Contracts`, create a new public static class with an `IServiceCollection` extension method, for example: `public static IServiceCollection AddVehicleApiClient(this IServiceCollection services, IConfiguration configuration)`.
    4.  **Implement the Extension Method**: This method should encapsulate all the logic for setting up the `HttpClient`:
        -   It should read the base URL and API key from the provided `IConfiguration`.
        -   It should call `services.AddHttpClient<IVehicleApiClient, VehicleServiceClient>(...)`.
        -   It should configure the `HttpClient` with the base address, a default timeout, and the API key header.
        -   It could also optionally include a default set of Polly resilience policies.
    5.  **Refactor the Consumer (`Insurance.Service`)**:
        -   Remove the now-redundant `VehicleServiceClient.cs` file and its interface from the `Insurance.Service` project.
        -   In `Insurance.Service/Program.cs`, replace the manual `AddHttpClient` block with a single call to the new extension method: `builder.Services.AddVehicleApiClient(builder.Configuration);`.
    6.  **Update Documentation**: Update the `README.md` to document this new, recommended pattern for service-to-service communication.

-   **Verification**:
    -   After refactoring, all existing tests should pass.
    -   The `Insurance.Service` should still be able to successfully call the `Vehicle.Service`.
    -   A developer wanting to consume the `Vehicle.Service` in a new project should only need to add the `Vehicle.Service.Contracts` NuGet package and call the `AddVehicleApiClient` extension method.
