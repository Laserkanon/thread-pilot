### Task: Add Load and Performance Tests for Batch Endpoint

-   **Priority**: Low
-   **Complexity**: High
-   **Description**: The batch endpoint (`POST /api/v1/vehicles/batch`) in `Vehicle.Service` is a critical performance path. To ensure it remains scalable and responsive under load, we need to introduce performance and load tests. This task involves using a load testing tool to simulate high traffic against this endpoint and measure key performance indicators (KPIs) like latency, throughput, and error rate.
-   **Affected Files**:
    -   A new project, e.g., `src/Vehicle/Vehicle.PerformanceTests/`

-   **Action Points**:

    1.  **Choose a Load Testing Tool**: Select a load testing tool. [NBomber](https://nbomber.com/) is a good choice as it allows you to write test scenarios in C# and can be integrated into the solution. Other options include k6 (JavaScript) or JMeter (GUI-based). The following steps will assume NBomber.
    2.  **Create a New Test Project**: Create a new C# project for performance tests: `src/Vehicle/Vehicle.PerformanceTests/`. Add the `NBomber` NuGet package to it.
    3.  **Write a Test Scenario**: Create a test scenario that simulates realistic load on the `Vehicle.Service`.
        -   The scenario should target the `POST /api/v1/vehicles/batch` endpoint.
        -   It should send a POST request with a payload containing a list of car registration numbers. The size of this list should be varied to test different batch sizes.
        -   The test should be configured to run with a specific number of virtual users (e.g., 50 concurrent users) for a certain duration (e.g., 1 minute).
        ```csharp
        // Example NBomber Scenario
        var scenario = Scenario.Create("vehicle_batch_load_test", async context =>
        {
            var requestBody = new { registrationNumbers = new[] { "REG123", "REG456" } };
            var request = Http.CreateRequest("POST", "http://localhost:5081/api/v1/vehicles/batch")
                              .WithJsonBody(requestBody);

            var response = await Http.Send(client, request);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1))
        );
        ```
    4.  **Define Assertions/Thresholds**: Configure NBomber to assert on performance thresholds. For example, the test should fail if:
        -   The 95th percentile (p95) response time exceeds 200ms.
        -   The error rate is greater than 1%.
    5.  **Integrate into Local Workflow**: Document in the `README.md` how to run these performance tests locally. Typically, this involves starting the services via `docker-compose` and then running the performance test project.
    6.  **(Optional) Integrate into CI**: Add a *manual* trigger to the CI pipeline (`.github/workflows/ci.yml`) to run the load tests. Load tests are often resource-intensive and long-running, so they should not typically run on every commit. They are usually run on-demand against a dedicated performance testing environment.

-   **Verification**:
    -   The performance test project can be run locally against the service.
    -   The test should generate a report (console output or HTML) showing KPIs like requests per second (RPS), latency (min, max, p95, p99), and error count.
    -   The test should fail if the defined performance thresholds are breached.
