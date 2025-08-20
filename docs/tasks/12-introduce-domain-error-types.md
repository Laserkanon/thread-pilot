### Task: Introduce Domain Error Types and Result Pattern

-   **Priority**: Medium
-   **Complexity**: High
-   **Description**: The current error handling relies on returning `null`, empty collections, or throwing generic exceptions. This makes it difficult for callers to understand *why* an operation failed without inspecting logs. This task involves introducing specific domain error types and a `Result` pattern to create a more explicit and robust error handling strategy. This makes the domain logic clearer and forces consumers of a service to handle failure scenarios explicitly.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Services/IInsuranceService.cs`
    -   `src/Insurance/Insurance.Service/Services/InsuranceService.cs`
    -   `src/Insurance/Insurance.Service/Controllers/InsurancesController.cs`
    -   A new file for the `Result` type, e.g., in a shared `Common` project or within each service.

-   **Action Points**:

    1.  **Define a Generic `Result<T>` Pattern**: Create a generic `Result<T>` class. This class will be used as the return type for service methods that can fail. It should contain either the successful result of type `T` or a specific error object.
        ```csharp
        // In a new file, e.g., Common/Result.cs
        public class Result<T>
        {
            public bool IsSuccess { get; }
            public T Value { get; }
            public DomainError Error { get; }

            // Private constructors, static factory methods for Success and Failure
            public static Result<T> Success(T value) => new Result<T>(value);
            public static Result<T> Failure(DomainError error) => new Result<T>(error);
        }

        public abstract record DomainError(string Message);
        ```
    2.  **Define Specific Domain Errors**: Create specific error types that inherit from `DomainError`. These will represent known business rule failures.
        ```csharp
        // In a new file, e.g., Insurance.Service/Models/Errors.cs
        public record PersonNotFound(string Pin) : DomainError($"Person with PIN '{Pin}' not found.");
        public record InvalidPinFormat(string Pin) : DomainError($"PIN '{Pin}' has an invalid format.");
        ```
    3.  **Refactor a Service Method**: Choose a method to refactor, for example `IInsuranceService.GetInsurancesForPinAsync`. Change its signature to return the new `Result` type.
        -   **Before**: `Task<IEnumerable<Models.Insurance>> GetInsurancesForPinAsync(string pin);`
        -   **After**: `Task<Result<IEnumerable<Models.Insurance>>> GetInsurancesForPinAsync(string pin);`
    4.  **Update Service Implementation**: In `InsuranceService.cs`, update the implementation to return `Result.Success(...)` on a happy path, or `Result.Failure(new PersonNotFound(...))` when no data is found for the given personal identity number.
    5.  **Update the Controller**: In `InsurancesController.cs`, update the call to the service. The controller must now explicitly handle the `Result` object. It should check `result.IsSuccess` and return the appropriate `IActionResult` (`Ok`, `NotFound`, `BadRequest`) based on the outcome. This moves the HTTP status code mapping logic cleanly into the controller.
        ```csharp
        var result = await _insuranceService.GetInsurancesForPinAsync(personalIdentityNumber);
        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                PersonNotFound err => NotFound(err.Message),
                _ => BadRequest("An unexpected error occurred.")
            };
        }
        return Ok(result.Value.Select(x => x.MapToContracts()));
        ```

-   **Verification**:
    -   Unit tests should be updated to reflect the new `Result<T>` return type and assert that the correct success or failure results are returned.
    -   When calling the API with a non-existent PIN, the controller should now return a `404 Not Found` based on the `PersonNotFound` domain error.
    -   When calling with a valid PIN, the API should return a `200 OK` with the insurance data.
