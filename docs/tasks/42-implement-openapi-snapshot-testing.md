### Task: Implement OpenAPI Snapshot Testing

-   **Priority**: Medium
-   **Complexity**: Low
-   **Description**: As a lightweight alternative or supplement to full contract testing with Pact, snapshot testing the generated `swagger.json` file can prevent accidental breaking changes to the API contract. This task involves creating a unit or integration test that compares the current `swagger.json` against a previously approved "snapshot" and fails the build if they don't match.
-   **Affected Files**:
    -   A new test class, e.g., in `Vehicle.IntegrationTests/ApiContractTests.cs`
-   **Action Points**:
    1.  **Choose a Snapshot Testing Library**: Add a snapshot testing library like `Verify.Xunit` to an integration test project (e.g., `Vehicle.IntegrationTests`).
    2.  **Create a Snapshot Test**:
        -   Write a new test that uses the `WebApplicationFactory` to start the service in-memory.
        -   Make an HTTP request to the `/swagger/v1/swagger.json` endpoint.
        -   Read the JSON content of the response.
        -   Use the snapshot library to compare the JSON content to a stored snapshot file.
        ```csharp
        // Example using Verify.Xunit
        [Fact]
        public async Task SwaggerContract_ShouldNotChange()
        {
            var client = _factory.CreateClient();
            var response = await client.GetStringAsync("/swagger/v1/swagger.json");
            await VerifyJson(response);
        }
        ```
    3.  **Generate the First Snapshot**: The first time the test is run, the snapshot library will create a `.verified.` file containing the `swagger.json` content. This file must be committed to the repository.
    4.  **Integrate into CI**: The snapshot test will run as part of the normal test suite in the CI pipeline. If a developer makes a change that alters the `swagger.json` (e.g., adds a new endpoint, changes a model), the test will fail.
    5.  **Update Workflow**: The developer must then review the changes to the `swagger.json`. If the change is intentional, they must update the snapshot file (usually by running a command provided by the snapshot library) and commit the updated snapshot along with their code changes.
-   **Verification**:
    -   A snapshot test for the OpenAPI specification should be present in one of the test projects.
    -   A `.verified.` snapshot file for the `swagger.json` should be committed to the repository.
    -   Making a change to a controller (e.g., adding a new parameter) should cause the snapshot test to fail until the snapshot is updated.
