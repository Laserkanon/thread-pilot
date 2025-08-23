### Task: Implement Batch Size Limiting (Chunking) for Vehicle Service Client

-   **Priority**: Medium
-   **Complexity**: Medium
-   **Description**: The current implementation of `GetVehiclesBatchAsync` sends all requested registration numbers in a single API call. If the number of car insurances for a person is very large, this could result in a request that is too large for the downstream `Vehicle.Service` to handle, potentially causing timeouts or `413 Request Entity Too Large` errors. To mitigate this, the client should be updated to "chunk" the requests.
-   **Action Points**:
    1.  **Introduce Batch Size Configuration**: Add a new configuration value, `Vehicle.Service.Client:MaxBatchSize`, to `appsettings.json` (e.g., default to `100`).
    2.  **Update Client Logic**: In `VehicleServiceClient.cs`, modify the `GetVehiclesBatchAsync` method to split the incoming list of registration numbers into smaller lists (chunks) based on the `MaxBatchSize`.
    3.  **Execute Multiple Batch Calls**: Use a loop (e.g., `Task.WhenAll` on a collection of tasks) to execute these smaller batch requests concurrently.
    4.  **Aggregate Results**: Aggregate the results from all the concurrent batch calls into a single collection to be returned.
    5.  **Update Unit Tests**: Add or modify unit tests to verify the new chunking logic, ensuring it correctly handles inputs both smaller and larger than the max batch size.
