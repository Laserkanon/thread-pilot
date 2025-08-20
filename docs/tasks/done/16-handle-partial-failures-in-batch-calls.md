### Task: Gracefully Handle Partial Failures in Batch API Calls

-   **Priority**: High
-   **Complexity**: High
-   **Description**: The `VehicleServiceClient` currently has `TODO` markers indicating it doesn't handle partial failures from the `Vehicle.Service`'s batch endpoint. If one registration number in a batch is invalid, the entire request might fail or return an empty set. This task involves improving the `Vehicle.Service` API and the `VehicleServiceClient` to gracefully handle partial failures, returning data for valid items and clearly indicating which items failed and why.
-   **Affected Files**:
    -   `src/Vehicle/Vehicle.Service/Controllers/VehiclesController.cs`
    -   `src/Vehicle/Vehicle.Service.Contracts/` (new response contract)
    -   `src/Insurance/Insurance.Service/Clients/VehicleServiceClient.cs`
    -   `src/Insurance/Insurance.Service/Services/InsuranceService.cs`

-   **Action Points**:

    **Part 1: Enhance the Provider (`Vehicle.Service`)**
    1.  **Define a Richer Response Contract**: In `Vehicle.Service.Contracts`, define a new response DTO for the batch endpoint that can represent partial success.
        ```csharp
        // Example in a new file Vehicle.Service.Contracts/BatchVehicleResponse.cs
        public class BatchVehicleResponse
        {
            public List<Vehicle> FoundVehicles { get; set; } = new();
            public List<FailedVehicleRequest> FailedRequests { get; set; } = new();
        }

        public class FailedVehicleRequest
        {
            public string RegistrationNumber { get; set; }
            public string Reason { get; set; } // e.g., "NotFound", "InvalidFormat"
        }
        ```
    2.  **Update Provider Endpoint Logic**: In `VehiclesController.cs` (in `Vehicle.Service`), update the batch endpoint logic. Instead of returning a simple array or a `404`/`400`, it should iterate through the requested registration numbers, sort them into "found" and "failed" lists, and return a `200 OK` or `207 Multi-Status` with the new `BatchVehicleResponse` payload.

    **Part 2: Update the Consumer (`Insurance.Service`)**
    3.  **Update Client Logic**: In `VehicleServiceClient.cs`, update the `GetVehiclesAsync` method to handle the new `BatchVehicleResponse`.
        -   It should deserialize the new response object.
        -   It should log the `FailedRequests` with their reasons for observability.
        -   It should return only the `FoundVehicles`.
    4.  **Update Service Logic**: In `InsuranceService.cs`, the `EnrichInsurancesWithVehicleDetailsAsync` method will now correctly receive only the details for vehicles that were actually found. The existing logic that iterates and matches vehicles to insurances will continue to work, but it will be more robust as it's no longer susceptible to a single bad input poisoning the entire batch.

-   **Verification**:
    -   Add unit tests in `Vehicle.UnitTests` to verify the new logic in the `VehiclesController`'s batch endpoint.
    -   Add or update unit tests in `Insurance.UnitTests` for `VehicleServiceClient`. Mock the `HttpClient` to return the new `BatchVehicleResponse` and assert that the client correctly parses it, logs failures, and returns only the successful results.
    -   Run an integration test by calling the `Insurance.Service` with a PIN that has multiple car insurances, where one has an invalid or non-existent registration number. Verify that the other car insurances are still correctly enriched with vehicle details.
