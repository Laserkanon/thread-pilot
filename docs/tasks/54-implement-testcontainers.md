### Task: Implement Containerized Integration Tests using Testcontainers

-   **Priority**: Medium
-   **Complexity**: High
-   **Description**: The current integration tests rely on a database being available at a specific connection string, which is typically a locally running Docker container managed outside the test runner. This creates a dependency on the developer's local environment. To make the integration tests more self-contained, reliable, and portable, this task is to integrate [Testcontainers](https://testcontainers.com/) to manage ephemeral database instances directly from the test code.
-   **Affected Files**:
    -   `src/Insurance/Insurance.IntegrationTests/`
    -   `src/Vehicle/Vehicle.IntegrationTests/`
    -   Potentially a new `Testing.Infrastructure` project.
-   **Action Points**:
    1.  **Add Testcontainers NuGet Package**: Add the `Testcontainers.MsSql` package to the integration test projects.
    2.  **Create a Test Fixture**: Create a test fixture (e.g., using xUnit's `IClassFixture` or `ICollectionFixture`) that will manage the lifecycle of the database container.
    3.  **Configure the Container**: In the fixture, define the MS SQL Server container using the Testcontainers API. This includes specifying the image, password, and waiting for the container to be healthy.
    4.  **Start and Stop the Container**: The fixture will start the container before the first test in a class/collection runs and stop/destroy it after the last test finishes.
    5.  **Inject Connection String**: The fixture should expose the dynamically generated connection string from the container. The integration tests will then be reconfigured to use this dynamic connection string to connect to the test database and run migrations.
    6.  **Refactor Tests**: Update the integration tests to use the new Testcontainers-based fixture.
-   **Verification**:
    -   The integration tests should run successfully without requiring a manually started database container.
    -   The tests should start a new, clean database container for each test run, ensuring test isolation.
    -   The CI pipeline should be able to run these tests without any special database setup steps.
