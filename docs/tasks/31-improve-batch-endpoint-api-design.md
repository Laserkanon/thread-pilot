### Task: Improve Batch Endpoint API Design to Report Invalid Inputs

-   **Priority**: Medium
-   **Complexity**: Medium
-   **Description**: The `POST /api/v1/vehicles/batch` endpoint currently accepts an array of registration numbers, validates them, and then silently discards any invalid numbers before fetching the valid ones. This behavior can hide upstream issues and lead to confusion for clients, as a `200 OK` is returned even if some requested data is missing due to invalid input. The API should be improved to explicitly report which inputs were invalid.
-   **Affected Files**:
    -   `src/Vehicle/Vehicle.Service/Controllers/VehiclesController.cs`
    -   `src/Vehicle/Vehicle.UnitTests/Controllers/VehiclesControllerTests.cs`
-   **Action Points**:
    1.  **Analyze Design Options**: Decide on a better API response strategy. Two common options are:
        -   **Option A (Strict)**: If any registration number in the batch is invalid, reject the entire request with a `400 Bad Request` and a response body detailing the invalid numbers.
        -   **Option B (Flexible)**: Return a `207 Multi-Status` response or a custom `200 OK` response object that contains both a list of found vehicles and a list of the invalid registration numbers with corresponding error messages.
    2.  **Implement New Response Logic**: Update the `GetVehiclesBatch` method in `VehiclesController.cs` to implement the chosen design. This will involve changing how the validation service is called and how the response is constructed.
    3.  **Update Unit Tests**: Modify the existing unit tests in `VehiclesControllerTests.cs` to assert the new, correct behavior. Add new tests for the scenarios where a mix of valid and invalid registration numbers are provided.
    4.  **Update API Documentation**: Ensure the Swagger/OpenAPI documentation for the endpoint is updated to reflect the new request and response format.
