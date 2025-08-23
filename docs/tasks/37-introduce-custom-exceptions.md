### Task: Introduce Custom Exception Types

-   **Priority**: Low
-   **Complexity**: Medium
-   **Description**: The application currently relies on default exception types and returns `NotFound()` or `BadRequest()` from controllers. To make the code more self-documenting and enable more robust, centralized error handling, custom exception types should be introduced for specific business logic errors.
-   **Affected Files**:
    -   `src/Vehicle/Vehicle.Service/Controllers/VehiclesController.cs`
    -   `src/Infrastructure/Hosting/PipelineHostingExtensions.cs` (or new middleware)
-   **Action Points**:
    1.  **Define Custom Exceptions**: Create a new set of custom exception classes in the `Infrastructure` project or a new shared `Common` project. Examples could include:
        -   `NotFoundException` (inherits from `Exception`)
        -   `ValidationException` (inherits from `Exception`, could contain a list of validation errors)
    2.  **Refactor Service/Repository Layer**: Throw the new custom exceptions from the service or repository layer when appropriate. For example, `VehicleRepository.GetVehicleByRegistrationNumberAsync` could throw a `NotFoundException` if no vehicle is found.
    3.  **Create Global Error Handling Middleware**: Create a new middleware that catches these specific custom exceptions and maps them to the appropriate HTTP status codes.
        -   Catch `NotFoundException` -> return `404 Not Found`.
        -   Catch `ValidationException` -> return `400 Bad Request` with the validation errors.
    4.  **Update `Program.cs`**: Add the new error handling middleware to the request pipeline in `Program.cs` for both services.
    5.  **Refactor Controllers**: Remove the manual `if (vehicleEntity == null)` checks from controllers and let the exceptions bubble up to the middleware. This will make the controllers thinner and cleaner.
    6.  **Verification**: Run all tests and manually test the API to ensure that the correct HTTP status codes and response bodies are returned for different error scenarios.
