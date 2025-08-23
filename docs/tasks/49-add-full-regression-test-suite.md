### Task: Add Full Environment Regression Test Suite

-   **Priority**: High
-   **Complexity**: Large
-   **Description**: To ensure the entire system works correctly as a whole, a suite of automated regression tests should be created. These tests will run against a fully deployed, production-like environment composed of all services running together, providing the highest level of confidence that the services interact correctly without any mocks.
-   **Affected Files**:
    -   A new project, e.g., `src/System.RegressionTests/`
    -   `.github/workflows/ci.yml`
-   **Action Points**:
    1.  **Create New Test Project**: Create a new test project named `System.RegressionTests`.
    2.  **Orchestrate Environment**: The test suite should be able to start all the services using the `docker-compose.local.yml` file. This can be done from the test runner itself (e.g., using `Testcontainers` to manage the compose file) or as a prerequisite step in the CI pipeline.
    3.  **Write End-to-End Scenarios**: Write tests that cover critical user flows from end to end. For example, a test could call the `Insurance.Service` to get insurances for a person and assert that the `vehicleDetails` are correctly enriched by the `Vehicle.Service`.
    4.  **Integrate into CI**: Add a new job to the CI pipeline (`.github/workflows/ci.yml`) to run these regression tests. This job should likely run only on pushes to the `main` branch or be manually triggered, as it will be slower and more resource-intensive than the unit and integration tests.

-   **Verification**:
    -   A new test project for regression tests should exist.
    -   The tests should successfully run against the multi-service environment orchestrated by Docker Compose.
    -   A change that breaks the interaction between services (e.g., a contract change in `Vehicle.Service` not yet adopted by `Insurance.Service`) should cause the regression tests to fail.
