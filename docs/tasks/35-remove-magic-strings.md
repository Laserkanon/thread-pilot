### Task: Remove Magic Strings from Code

-   **Priority**: Medium
-   **Complexity**: Low
-   **Description**: The codebase contains several "magic strings," which are hardcoded string literals used for things like URLs or configuration keys. This practice is error-prone and makes the code harder to maintain. These strings should be replaced with named constants.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Clients/VehicleServiceClient.cs`
    -   `src/Infrastructure/Hosting/ApiKeyAuthHandler.cs` (already uses a constant, good example)
-   **Action Points**:
    1.  **Identify Magic Strings**: Review the codebase, paying special attention to client and handler classes, to find hardcoded string literals. A key example is the URL `"/api/v1/vehicles/batch"` in `VehicleServiceClient.cs`.
    2.  **Define Constants**: For each magic string, define a `const string` with a descriptive name. These constants should be placed in a relevant class. For the client URL, a private constant within `VehicleServiceClient.cs` would be appropriate.
    3.  **Replace Literals**: Replace all occurrences of the hardcoded string literal with a reference to the new constant.
    4.  **Verification**: Rebuild the solution and run all tests to ensure that the refactoring did not change the application's behavior.
