### Task: Implement Negative-Path Tests for Service Client

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: The current test suite primarily covers "happy path" scenarios. To build a robust system, we must test how it behaves during failures. This task involves adding tests for negative paths, specifically for the `VehicleServiceClient`. The `TODO` comments in the client indicate that handling of partial failures and service unavailability is not well-defined. These tests will drive the implementation of more resilient error handling.
-   **Affected Files**:
    -   `src/Insurance/Insurance.UnitTests/Clients/VehicleServiceClientTests.cs` (or a new file for it)
    -   `src/Insurance/Insurance.UnitTests/Services/InsuranceServiceTests.cs`
    -   `src/Insurance/Insurance.Service/Clients/VehicleServiceClient.cs` (to implement the required logic)
-   **Action Points**:
    1.  **Create Test Scenarios**: In `Insurance.UnitTests`, create new unit tests for the `InsuranceService`. These tests should mock the `IVehicleServiceClient` to simulate various failure conditions:
        -   The `GetVehiclesAsync` method throws an `HttpRequestException` (simulating network error or service down).
        -   The `GetVehiclesAsync` method returns an empty list even when valid registration numbers are sent.
        -   The `GetVehiclesAsync` method returns a `null` response.
        -   The `GetVehiclesAsync` method returns a partial list (e.g., only 2 out of 3 requested vehicles are found).
    2.  **Assert Correct Behavior**: For each test case, assert that the `InsuranceService` behaves correctly. For example:
        -   If the vehicle service is down, does the call to `GetInsurancesForPinAsync` fail gracefully or does it crash? The desired behavior should be defined (e.g., return insurance data without enrichment, log a warning).
        -   If a partial response is received, does it correctly enrich the policies it can and ignore the ones it cannot?
    3.  **Implement Handler Logic**: Based on the failing tests, implement the necessary error handling and fallback logic within `VehicleServiceClient.cs` and/or `InsuranceService.cs`. This will involve removing the existing `TODO`s and replacing them with robust code.
    4.  **(Optional) Integration Tests with Mock Server**: For a higher level of confidence, consider adding an integration test in `Insurance.IntegrationTests`. Use a library like `WireMock.Net` to create a mock `Vehicle.Service` HTTP server that can be configured to return `500` errors, timeouts, or malformed responses, and assert that the `Insurance.Service` handles these scenarios correctly during a real HTTP call.
    5.  **Verification**: Ensure all new tests pass after implementing the error handling logic. The tests should clearly document the expected behavior under failure conditions.
