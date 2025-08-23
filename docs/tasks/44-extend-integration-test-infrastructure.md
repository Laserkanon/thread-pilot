### Task: Extend Infrastructure Libraries for Integration Test Setup

-   **Priority**: Medium
-   **Complexity**: High
-   **Description**: The current integration tests have duplicated setup logic for creating `WebApplicationFactory` instances and configuring test-specific services. To make writing integration tests easier, more consistent, and less repetitive, a shared infrastructure library for testing should be created.
-   **Affected Files**:
    -   A new project, e.g., `src/Testing/Testing.Infrastructure/`
    -   `src/Insurance/Insurance.IntegrationTests/`
    -   `src/Vehicle/Vehicle.IntegrationTests/`
-   **Action Points**:
    1.  **Create Shared Test Library**: Create a new shared class library project named `Testing.Infrastructure`.
    2.  **Identify Common Setup Code**: Review the setup code in the existing integration test projects to identify common patterns (e.g., `WebApplicationFactory` instantiation, client creation, database connection management).
    3.  **Create Reusable Test Fixtures**: In the new `Testing.Infrastructure` project, create reusable test fixtures or base classes. For example, a `BaseIntegrationTest` class could encapsulate the logic for:
        -   Creating and configuring the `WebApplicationFactory`.
        -   Setting up test-specific configurations (e.g., for in-memory databases or Testcontainers).
        -   Providing a pre-configured `HttpClient` for making requests to the service under test.
    4.  **Refactor Existing Tests**: Update the `Insurance.IntegrationTests` and `Vehicle.IntegrationTests` to inherit from or use the new shared test fixtures, removing duplicated code.
    5.  **Integrate Testcontainers (Optional)**: This new library would be the ideal place to centralize the use of `Testcontainers` (as described in another task) to provide a unified way to spin up dependencies like databases for all integration tests.

-   **Verification**:
    -   The new `Testing.Infrastructure` project should be created and referenced by the integration test projects.
    -   The existing integration tests should be refactored to use the shared library.
    -   All integration tests must continue to pass after the refactoring.
