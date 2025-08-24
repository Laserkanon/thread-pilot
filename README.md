# ThreadPilot Integration Layer

This repository contains the solution for the ThreadPilot integration layer assignment. The project demonstrates a robust, maintainable, and scalable approach to integrating a new core system with legacy systems using a microservice architecture in .NET.

It consists of two primary services:
-   **`Vehicle.Service`**: Manages and provides data about vehicles.
-   **`Insurance.Service`**: Manages insurance policies and integrates with `Vehicle.Service` to provide enriched data.

This document provides a comprehensive overview of the architecture, setup instructions, and key design decisions.

---

## 1. Local Development and Testing

This guide covers how to set up and run the project on your local machine for development and testing.

### 1.1. Prerequisites

* **.NET 8 SDK**
* **PowerShell 7+**
* **Docker Engine**


### 1.2. Configure Secrets (One-Time Setup)

Before running the application, you need to configure your local secrets. This project uses a `secrets.local.json.template` file as a blueprint. You'll create your own `secrets.local.json` file (which is git-ignored) and then run a script to apply these secrets to both Docker and non-Docker environments.

1.  **Create the secrets file**: In the project's root directory, make a copy of `secrets.local.json.template` and rename it to **`secrets.local.json`**.
    ```bash
        cp secrets.local.json.template secrets.local.json
    ```

2.  **Edit the secrets file**: Open **`secrets.local.json`** and change your secret values.

    ```json
    {
      "Insurance.Service": {
        "DB_SA_PASSWORD": "YourStrongPassword!123",
        "Authentication": {
          "ApiKey": "d3a8b273-5a41-47c3-9a70-7981b1c3a6e8"
        },
        "Vehicle.Service.Client":{
          "ApiKey": "d3a8b273-5a41-47c3-9a70-7981b1c3a6e8"
        }
      },
      "Vehicle.Service": {
        "DB_SA_PASSWORD": "YourStrongPassword!123",
        "Authentication": {
          "ApiKey": "d3a8b273-5a41-47c3-9a70-7981b1c3a6e8"
        }
      },
      "Insurance.Db": {
        "DB_SA_PASSWORD": "YourStrongPassword!123"
      },
      "Vehicle.Db": {
        "DB_SA_PASSWORD": "YourStrongPassword!123"
      }
    }
    ```

3.  **Run the initialization script**: Open a PowerShell terminal in the root directory and execute:
    ```powershell
    ./init.ps1
    ```
    This script automatically populates a `.env` file for Docker and configures the .NET Secret Manager for running the services directly. It will also access dotnet dev certificate.

---

### 1.3. Run the Application

After configuring secrets, choose one of the following methods to run the services.

#### Option A: Docker Compose (Quickest for Testing) 🚀

This is the **fastest way to get the entire application running**, especially for integration testing or a quick validation. It handles all dependencies, including the database, with a single command.

1.  Make sure your **Docker Engine is running**.
2.  From the project's root directory, run:
    ```bash
    docker compose -f docker-compose.local.yml up --build
    ```
    This command builds the service images and starts containers for both services and a shared SQL Server database. Database migrations are applied automatically on startup.

**Accessing the Services (Docker):**
* **Vehicle Service**: `http://localhost:5081` ([Swagger UI](http://localhost:5081/swagger))
* **Insurance Service**: `http://localhost:5082` ([Swagger UI](http://localhost:5082/swagger))

---

#### Option B: Manual Setup (Recommended for Development/Debugging) 🛠️

This method is **recommended for active development and debugging**. Running the services directly on your host machine provides the best integration with your IDE (like Visual Studio or VS Code), enabling features like hot reload and an attached debugger.

First, ensure you have completed the **2.2. Configure Secrets** step above, as the `init.ps1` script is required to set up secrets for local execution.

1.  **Set Up the Database & Run Migrations**: Run the `init-db.ps1` script. This single command will start the SQL Server Docker container and then execute the database migrations. The migration applications will automatically wait for the database to be ready before applying changes.
    ```powershell
    ./init-db.ps1
    ```

2.  **Start the Services**: The recommended method is to launch both the **`Vehicle.Service`** and **`Insurance.Service`** projects from your preferred IDE (like Visual Studio or Rider). This provides full debugging support with breakpoints and hot reload, and you may need to configure your IDE to launch multiple startup projects.

    <details>
    <summary>Click here for terminal commands</summary>

    If you prefer the command line, open two separate terminals and run the following commands.

    **In Terminal 1:**
    ```bash
    cd src/Vehicle/Vehicle.Service
    dotnet run
    ```

    **In Terminal 2:**
    ```bash
    cd src/Insurance/Insurance.Service
    dotnet run
    ```
    </details>

**Accessing the Services (Manual):**
* **Vehicle Service**: `https://localhost:7297` ([Swagger UI](https://localhost:7297/swagger))
* **Insurance Service**: `https://localhost:7296` ([Swagger UI](https://localhost:7296/swagger))

---

### 1.4. Manual Testing with Swagger UI

Once the services are running, you can use the interactive Swagger UI pages to test the API endpoints. The database migrations seed the database with sample data for you to use.

#### API Key Authentication

The API endpoints are protected and require an API key to be sent in the request header.

1.  Find your API key in the **`secrets.local.json`** file. The default key is `d3a8b273-5a41-47c3-9a70-7981b1c3a6e8`.
2.  On the Swagger UI page, click the **Authorize** button, usually located in the top-right corner.
3.  In the dialog box that appears, paste your API key into the **Value** field and click **Authorize**.
4.  Close the dialog. Swagger will now automatically include your API key in the header for all subsequent requests.

#### Available Test Data & Endpoints

**Vehicles.Service** seeded registration numbers:
* `ABC123`
* `GHI789`
* `DEF456`

**Insurance.Service** seeded personal identification numbers:
* `199001011234` (has registration number)
* `198505155678` (has registration number)
* `200310209876`

### 1.5. Running Automated Tests

To run all unit and integration tests across the solution, execute the following command from the root directory:

```
dotnet test
```
You can also run tests for a specific project, for example:
```
cd src/Insurance/Insurance.UnitTests
dotnet test
```

---

## 2. Architecture and Design Decisions

The solution is built using a distributed microservice architecture to ensure a clear separation of concerns, independent scalability, and deployment flexibility.

### 2.2. Microservice Architecture

-   **`Vehicle.Service`**: A focused microservice that exposes a REST API for vehicle data. It has its own dedicated database, managed by **DbUp**.
-   **`Insurance.Service`**: A microservice that provides insurance information for individuals. It performs internal orchestration by calling `Vehicle.Service` to enrich car insurance policies with vehicle details.

### 2.2. Vehicle Data Enrichment Strategy

Our primary architectural assumption is that the downstream Vehicle.Service is a legacy system. As such, its load capacity is unknown, and we cannot assume it can be easily extended. Our entire data enrichment strategy is therefore built defensively to respect these constraints.

The default and safest method for fetching vehicle data is to make concurrent, distinct calls to the GetVehicleAsync(registrationNumber) endpoint. This approach avoids placing sequential calls that would have a risk of timing out or be too slow, while also limiting the vehicle service to only look up what it really must by using distinct registration numbers. To prevent overwhelming the Vehicle.Service, the level of concurrency is strictly controlled by the MaxDegreeOfParallelism configuration value, which acts as a vital safety valve. As we mentioned, the vehicle service's performance is unknown, and we placed a configurable parameter with a value of 5 concurrent requests as a starting point, but it can be easily configured depending on the load. Furthermore, all HTTP communication is wrapped in resilience policies, such as Retry, Circuit Breaker and Fallbacks, to handle transient network errors and service unavailability gracefully.

However, it is unclear how difficult it would be to change the legacy system. We imagined that if a developer on that team could expose a more efficient endpoint, let's say a batch variant, we would prefer to use it, as it would significantly reduce the overhead of multiple HTTP requests and also increase database efficiency (single vs. multiple lookups). To demonstrate this pattern, we have implemented a client method (GetVehiclesBatchAsync) that could consume such an endpoint.

To provide flexibility and safety, this entire process is controlled by two distinct feature toggles: `EnableVehicleEnrichment` and `EnableBatchVehicleCall`.

#### Configuration
All parameters for the vehicle data enrichment strategy are configured in the `appsettings.json` file of the `Insurance.Service`.

- **`Vehicle.Service.Client:MaxDegreeOfParallelismSingle`**: This integer controls the maximum number of concurrent single-call requests sent to `Vehicle.Service`. It is a critical safety valve to prevent overwhelming a legacy system. The default value is `5`.

- **`Vehicle.Service.Client:MaxDegreeOfParallelismBatch`**: This integer controls the maximum number of concurrent batch requests sent to `Vehicle.Service`. Its default value is `1` to ensure that even batch calls are sent sequentially unless explicitly configured otherwise.

- **`Vehicle.Service.Client:MaxBatchSize`**: This integer defines the maximum number of registration numbers that can be included in a single batch request to `Vehicle.Service`. The default value is `50`.

- **`FeatureToggles:EnableVehicleEnrichment`**: A boolean that acts as a global kill-switch for the entire enrichment process. If `false`, no calls will be made to `Vehicle.Service`. Defaults to `true`.

- **`FeatureToggles:EnableBatchVehicleCall`**: A boolean that toggles between the two enrichment strategies. If `true`, the system will use the batch endpoint (`GetVehiclesBatchAsync`). If `false`, it will use concurrent single calls (`GetVehicleAsync`). Defaults to `false`.

### 2.3. Dependency Injection (DI)

The project uses the standard, built-in .NET dependency injection container. The setup is clean, straightforward, and configured directly in `Program.cs`, following modern .NET best practices.

The majority of application and data access services are registered with a **Scoped** lifetime. This is a deliberate and standard practice for web applications for several key reasons:
-   **Safety**: A new instance of the service is created for each HTTP request. This is the safest way to handle dependencies that use resources like database connections, as it prevents data from one user's request from ever leaking into another's.
-   **Efficiency**: Within the same HTTP request, the same instance of a service is shared. If multiple components need the same service during a single operation, they all receive the same instance, avoiding the overhead of creating new objects unnecessarily.
-   **Correctness**: It ensures that a single, consistent "unit of work" is used for the duration of a request. For example, all repository interactions during one API call can share the same database transaction context.

Other lifetimes are used where appropriate. For example, the **typed `HttpClient` pattern** is used for communication between services. When using `services.AddHttpClient<T>()`, the typed client itself is registered as **transient**, but it uses an `HttpClient` managed by the `IHttpClientFactory`. This factory is crucial for two reasons:
-   **Connection Management**: It pools and reuses the underlying network connections (`HttpMessageHandler`s), which prevents socket exhaustion and improves performance.
-   **Policy Integration**: It seamlessly integrates with resilience policies (like Retry and Circuit Breaker from Polly). The factory ensures that the configured policies are correctly applied to every request made by the `HttpClient`.

This combination provides a robust, efficient, and resilient way to make HTTP requests.

*Example from `Insurance.Service/Program.cs`:*
```csharp
// Domain services are registered with a Scoped lifetime
builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();

// Typed HttpClient for Vehicle Service (managed by the factory)
builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>();
```

### 2.4. Data Access

**Dapper** was chosen as the micro-ORM for data access. It offers high performance and a lightweight abstraction over raw ADO.NET without the complexity of a full ORM like Entity Framework, which was deemed unnecessary for this project's scope. Database migrations are handled by **DbUp**, ensuring version-controlled and repeatable schema changes.

### 2.5. Input Validation

**FluentValidation** is used for validating API inputs. This choice promotes a clean and unified approach to validation logic, separating it from the core business logic of the controllers and services. It provides a robust way to define complex validation rules and results in a clear separation of concerns.

### 2.6. Shared Contracts

The solution uses dedicated `.Contracts` projects (e.g., `Insurance.Service.Contracts`) to define the public data models (Data Transfer Objects or DTOs) that are shared between services. This is a critical architectural pattern for several reasons:

-   **Explicit Public Interface**: It creates a clear separation between the service's internal data models (how data is stored in the database) and its public contract (how data is exposed to the outside world). This prevents accidentally exposing internal fields or implementation details, which is important for both security and maintainability.
-   **Improved Developer Experience (DX)**: When a developer needs to change an internal data model, the compiler will force them to consider how that change impacts the public contract, as the mapping will need to be updated. This makes refactoring safer and prevents unintended breaking changes.
-   **Service Decoupling**: A service (like `Insurance.Service`) can consume the contract of another service (`Vehicle.Service.Contracts`) without needing a dependency on its full implementation, reducing coupling and improving build times.
-   **Clear Versioning**: By having the API contract defined in a separate assembly, it becomes easier to manage versioning and support multiple versions of a contract simultaneously in the future.

### 2.7. Authentication and Authorization

The services currently use a simple API Key-based authentication mechanism. This choice was made because the overall architecture and usage patterns of these services are not yet fully defined. Implementing API Key authentication provides a cost-effective and minimal first step to secure the endpoints, while keeping flexibility for more advanced authentication and authorization approaches as the system evolves.

-   **How it Works**: Each service is configured with a secret API key. Clients (including other services within the solution) must include this key in the `X-Api-Key` header of their HTTP requests. A custom authentication handler (`ApiKeyAuthHandler`) validates the key using a constant-time comparison algorithm to protect against timing attacks. If the key is missing or invalid, the request is rejected with a `401 Unauthorized` response.
-   **Configuration**: The API key for each service is defined in `secrets.local.json` and applied via the `init.ps1` script, which stores them in .NET User Secrets. This ensures that keys are not hardcoded and are managed securely during local development. The `Vehicle.Service` client within the `Insurance.Service` is also configured with the correct key via dependency injection.
-   **Security**: This strategy provides a solid layer of security for internal services, ensuring that only authorized clients can access the APIs. All endpoints across all services are protected by this scheme by default.

This pragmatic approach was chosen for its simplicity. It is currently unclear whether this service will be exposed directly to end-users or will only be used for internal, system-to-system communication. This ambiguity must be resolved before committing to a more sophisticated, user-facing authentication solution like JWT, as a thorough investigation into the service's definitive role could save significant implementation time.

### 2.8. Feature Toggles

The solution uses a generic and reusable feature toggle system to enable or disable certain functionality at runtime without requiring a redeployment. The implementation is based on the following principles:

-   **Generic Service**: A generic `IFeatureToggleService<T>` is provided in the `Infrastructure` project, where `T` is a class that defines the feature toggle properties for a specific consumer.
-   **Configuration-based**: Each consumer defines its own settings class (e.g., `InsuranceFeatureToggles`) which is bound to the fixed `FeatureToggles` configuration section in `appsettings.json`.
-   **Easy Setup**: A generic hosting extension, `AddFeatureToggles<T>()`, makes it easy to register the feature toggles for a service. It automatically binds to the `FeatureToggles` section in the configuration.
-   **Dynamic Reloading**: The implementation uses `IOptionsMonitor<T>` to read the toggle values, which allows for dynamic, real-time reloading of feature toggles from `appsettings.json` without an application restart.

---



## 3. Security and TLS Termination Strategy

For the local development environment, this project prioritizes simplicity and ease of use to ensure developers can get up and running quickly.

-   **When using Docker Compose**, the services run on plain **HTTP**. This is intentional, as it removes the complexity of managing SSL certificates for the Docker environment.
-   **When running locally with `dotnet run`**, the services use the standard **ASP.NET Core HTTPS development certificates**.

In a real-world **production environment**, you would not expose the services directly. Instead, you would implement **TLS Termination** at the edge of your network. This is typically handled by a reverse proxy, an API Gateway, or a load balancer (e.g., Nginx, YARP, Azure Application Gateway).

This approach has several benefits:
-   **Centralized TLS Management**: SSL certificates are managed in one place, simplifying deployment and renewal.
-   **Improved Security**: The internal network traffic between the gateway and the services can be on plain HTTP within a secure, private network, reducing the attack surface.
-   **Simplified Services**: The application services themselves do not need to be concerned with the complexities of certificate management.

The goal of this repository is not to implement TLS termination, but rather to produce a container. The responsibility for TLS termination is packaged in a separate step/process.

---

## 4. Technical Approach Details

### 4.1 Testing Strategy

The solution includes both unit and integration tests to ensure correctness and stability.
-   **Unit Tests**: These focus on testing individual components in isolation. For example, the `InsuranceService` unit tests mock both the database repository and the `IVehicleServiceClient` to verify the service's logic without making real HTTP calls or database queries. This is a standard practice to ensure tests are fast, focused, and completely isolated from external dependencies.
-   **Integration Tests**: These tests validate the interaction between Insurance.Service and its direct dependencies. They start up the service together with its test database, then perform a real HTTP call from Insurance.Service through to the database and back, ensuring the full end-to-end data flow works as expected. External microservice dependencies are mocked. While mocking isn’t strictly necessary, the author suggests introducing an additional, broader test project at a third level to keep these tests focused and fast, while still allowing more comprehensive scenarios to be verified separately without slowing down the core integration suite.

### 4.2 Error Handling

-   **Invalid Input (`400 Bad Request`)**: API inputs are validated using **FluentValidation**. If validation fails, the API returns a `400` response with clear error messages.
-   **Missing Data (`404 Not Found`)**: If a resource like a vehicle or a person's insurance profile is not found, the API returns a `404`.
-   **Graceful Degradation**: If the `Vehicle.Service` is called for a specific car insurance policy but does not find a matching vehicle, it returns `null` for that vehicle's details. This is a **deliberate design choice** to ensure that the entire request for a person's insurances doesn't fail just because one vehicle record is missing. The system gracefully degrades rather than failing completely.

### 4.3 Extensibility and API Versioning

-   The service-oriented architecture allows new services to be added with minimal impact on existing ones.
-   The use of repository and service abstractions makes it easier to modify or replace implementations (e.g., swapping out the database).
-   All APIs are versioned from the start (`/api/v1/...`) to allow for future, non-breaking changes.

### 4.4 Observability

The solution includes basic observability features out of the box.

-   **Health Checks**: Both services expose a standard health check endpoint at `/healthz`. This endpoint is intentionally left unauthenticated to allow seamless integration with automated health monitoring systems like Kubernetes or load balancers, which typically do not use authentication headers for their probes.
-   **Application Metrics**: Both services use **OpenTelemetry** to expose a wide range of application metrics in a **Prometheus**-compatible format. This includes standard metrics for HTTP requests (e.g., duration, count, status codes) and HttpClient calls. The metrics are available on the `/metrics` endpoint of each service and can be scraped by a Prometheus server for monitoring and alerting.
-   **Structured Logging**: The services are configured to use **Serilog** for structured logging. Logs are written to the console in a machine-readable JSON format, which makes them easy to collect, parse, and analyze in a centralized logging platform.

### 4.5 Resilience

The `Insurance.Service` implements key resilience patterns using **Polly** for its calls to the `Vehicle.Service`. This makes the system more robust against transient failures and service unavailability. The following policies are in place:
-   **Retry Policy**: Automatically retries failed HTTP requests with an exponential backoff strategy to handle temporary network issues or service hiccups.
-   **Circuit Breaker**: Prevents the service from repeatedly calling a known-to-be-unhealthy `Vehicle.Service`. After a configurable number of consecutive failures, the circuit "opens," and subsequent calls fail immediately for a set duration, allowing the downstream service time to recover.
-   **Fallback Policy**: When the circuit is open, a fallback mechanism provides a default response (an empty list of vehicles) instead of throwing an exception. This ensures that the `Insurance.Service` can still function gracefully and provide a partial response to its clients even when its dependency is down.

---

## 5. CI/CD

A basic Continuous Integration (CI) pipeline is defined in `.github/workflows/ci.yml`. This GitHub Actions workflow triggers on every push and pull request to automatically build the solution, run the tests, and ensure the codebase remains in a healthy state. On pushes to any branch, it also publishes NuGet packages for the service contracts and Docker images for the services to the GitHub Package Registry, using a dynamic versioning strategy for feature branches.

## 6 AI-Assisted Development

Most of the code in this solution has been written by AI assistants, including ChatGPT-5 and the agent Google Jules. The developer's role was focused on performing critical reviews, making architectural decisions, and identifying and fixing subtle issues that the AI was not able to see. The developer also performed manual programming when required.

A good example of a necessary human correction was in solving the N+1 problem when calling the Vehicle service. An initial AI-generated solution proposed fetching vehicle data for each registration number individually and also missed applying a `Distinct()` operation on the list of registration numbers. While the speed and parallel work provided by AI were beneficial, human oversight was crucial for ensuring the final implementation was both correct and performant.

---

## 7. Personal Reflection

**Any similar project or experience you’ve had in the past:**

Yes, I recognize the issues in this assignment and the complexity they bring. It’s quite rare to get the opportunity to build everything from scratch, which really highlights the importance of strong platform libraries and how much they can accelerate development.

**What was challenging or interesting in this assignment:**

The most interesting challenge was designing the integration with the `Vehicle.Service` under the assumption that it's a legacy system with unknown performance and reliability. This constraint forces a **defensive design strategy**, which is a critical aspect of real-world systems integration.

Drawing conclusions from the Vehicle Data Enrichment Strategy, the key takeaway is the importance of building a system that is both resilient and adaptable. Instead of making risky assumptions, the solution uses multiple layers of protection:
-   Strictly controlling concurrency (`MaxDegreeOfParallelism`) to avoid overwhelming the downstream service.
-   Wrapping all communication in resilience policies (Retry, Circuit Breaker) to handle transient failures gracefully.
-   Providing flexibility through feature toggles (`EnableVehicleEnrichment`, `EnableBatchVehicleCall`), which act as safety valves and allow the integration strategy to evolve without requiring a new deployment. This demonstrates a pragmatic approach to managing dependencies on systems outside of our direct control.

**What you would improve or extend if you had more time:**

While there is a long list of potential technical tasks in the "Future Improvements" section, the most critical next step would not be to simply start coding. Instead, I would prioritize a deeper **investigation into the current state and future roadmap of the overall system**.

Making informed decisions requires more context. For instance, understanding the actual load capacity of `Vehicle.Service` would allow for fine-tuning the concurrency limits. Clarifying whether a batch endpoint is a realistic future development for the legacy team would determine if our `EnableBatchVehicleCall` strategy is a short-term or long-term solution. Similarly, identifying the definitive consumers of this integration layer (internal systems vs. external users) is essential for choosing the right long-term security model (e.g., API Keys vs. JWT etc).

---

## 8. Future Improvements

This section outlines potential enhancements to the solution, categorized for clarity.

**NOTE: this list is not final and there are many additional things to consider.**

### 8.1. Observability and Resilience
-   **Distributed Tracing**: Integrate a distributed tracing solution like OpenTelemetry to provide end-to-end visibility of requests as they travel across services, making it easier to diagnose latency and errors.
-   **Future Resilience Enhancements**: Further resilience patterns like **Bulkhead** isolation and **Rate Limiting** could be added to provide even greater protection against cascading failures.

### 8.2. Architecture and Design
-   **Event-Driven Architecture**: Explore evolving the architecture to incorporate asynchronous messaging (e.g., with RabbitMQ or Kafka). This would decouple services further and enable patterns like **CQRS** and **Sagas** for more complex workflows.
-   **Native AOT Compilation**: Investigate compiling the services to **Native AOT** (Ahead-of-Time) to significantly reduce memory footprint and startup times, making the services more efficient and scalable, especially in containerized environments.
-   **API Client Generation**: Automate the creation of the `VehicleServiceClient` by generating it directly from the `Vehicle.Service`'s OpenAPI/Swagger specification. This ensures the client is always in sync with the API contract.
-   **Enhanced API Documentation**: Improve the existing Swagger documentation by adding detailed XML comments (`<summary>`, `<param>`, `<returns>`) to the API controllers and models. This would provide clearer, auto-generated guidance for API consumers.
-   **Dynamic Feature Toggles**: For a more advanced setup, integrate a centralized feature toggle management service like LaunchDarkly or Azure App Configuration for real-time control over features in production.

### 8.3. Security
-   **Centralized Secret Management**: Refactor the solution to remove secrets (like database passwords) from `appsettings.json` and `docker-compose.yml` files. Instead, integrate a proper secret management tool like HashiCorp Vault or Azure Key Vault. This addresses the concern of having passwords committed to the repository and prepares the application for secure production deployments.
-   **Secret Scanning**: Integrate automated secret scanning tools (like Gitleaks) into the CI pipeline to prevent sensitive information like API keys or credentials from being accidentally committed to the repository.

### 8.4. Testing and Quality Assurance
-   **Contract Testing**: Formalize the API contracts between services using a framework like Pact. This would ensure that changes to `Vehicle.Service` do not break `Insurance.Service`'s expectations, catching integration issues early in the development cycle.
-   **Code Quality and Coverage**: Integrate **SonarCloud** or a similar static analysis tool into the CI pipeline to enforce code quality standards and track test coverage over time. *(Note: The CI pipeline already generates a coverage report, but no quality gates are enforced.)*
-   **Infrastructure as Code (IaC)**: Use a tool like **Terraform** to define and manage the cloud infrastructure required for staging and production environments, ensuring consistency and repeatability.
-   **Containerized Integration Tests**: Use **Testcontainers** to spin up ephemeral database instances for integration tests, further isolating them from the local development environment.
-   **Full Environment Regression Tests**: Create a dedicated test suite that uses Docker Compose to run all services together. This would enable testing of the entire system in a simulated full environment, providing the highest level of confidence that the services interact correctly without mocks.

### 8.5. CI/CD and Deployment
-   **Advanced Deployment Strategies**: Enhance the CI/CD pipeline to support advanced deployment strategies like **Blue-Green deployments** or **Canary Releases**. This would minimize downtime and risk during production releases.

### 8.6. Repository structure

- **Consider mono vs seperation by bounded service**

---

## 9. Task Estimates and Future Work

The following is a high-level, categorized list of the pending tasks documented in the `docs/tasks` directory. This list can be used for prioritization and planning.

**Legend**:
*   **S (Small)**: ~1-2 hours
*   **M (Medium)**: ~2-4 hours
*   **L (Large)**: ~4-8 hours
*   **XL (Extra Large)**: > 8 hours

### 9.1. Security
*   **Total Estimated Effort**: ~1.5 days
*   **Tasks**:
    *   `[M]` **Task 40**: Implement API Quotas and Rate Limiting
    *   `[M]` **Task 46**: Integrate Azure Key Vault
    *   `[M]` **Task 53**: Integrate Secret Scanning

### 9.2. Architecture & Refactoring (Improves Code Quality)
*   **Total Estimated Effort**: ~3-4 days
*   **Tasks**:
    *   `[S]` **Task 32**: Refactor Controller Dependencies
    *   `[S]` **Task 33**: Cleanup Project Dependencies
    *   `[S]` **Task 34**: Apply Async Best Practices
    *   `[S]` **Task 35**: Remove Magic Strings
    *   `[S]` **Task 43**: Configure HttpClient Properties
    *   `[M]` **Task 31**: Improve Batch Endpoint API Design
    *   `[M]` **Task 48**: Expose Client in Contract Package
    *   `[L]` **Task 12**: Introduce Domain Error Types (Result Pattern)
    *   `[L]` **Task 37**: Introduce Custom Exception Types

### 9.3. Testing & Quality Assurance (Improves Reliability)
*   **Total Estimated Effort**: ~5-6 days
*   **Tasks**:
    *   `[M]` **Task 42**: Implement OpenAPI Snapshot Testing
    *   `[L]` **Task 06**: Add Load and Performance Tests
    *   `[L]` **Task 44**: Extend Integration Test Infrastructure
    *   `[L]` **Task 49**: Add Full Environment Regression Test Suite
    *   `[L]` **Task 54**: Implement Containerized Integration Tests (Testcontainers)

### 9.4. CI/CD & DevOps (Improves Automation)
*   **Total Estimated Effort**: ~5-6 days
*   **Tasks**:
    *   `[S]` **Task 10**: Add Placeholder Deployment Step to CI
    *   `[S]` **Task 20**: Create IaC Stubs
    *   `[M]` **Task 08**: Add Quality Gates to CI (Formatting/Static Analysis)
    *   `[M]` **Task 23**: Visualize Code Coverage in CI (e.g., Codecov)
    *   `[M]` **Task 36**: Optimize Dockerfiles
    *   `[M]` **Task 39**: Create Alpine Docker Images
    *   `[M]` **Task 47**: Publish Database Migrator Images
    *   `[L]` **Task 24**: Implement Automated Semantic Versioning
    *   `[L]` **Task 52**: Investigate Native AOT Compilation
    *   `[XL]` **Task 45**: Dynamically Generate Docker Compose File

### 9.5. New Features & Observability
*   **Total Estimated Effort**: ~3-4 days
*   **Tasks**:
    *   `[S]` **Task 02**: Add Metrics for Feature Toggles
    *   `[M]` **Task 18**: Add Distributed Tracing
    *   `[M]` **Task 50**: Implement improved Bulkhead Resilience Pattern
    *   `[L]` **Task 55**: Integrate Centralized Feature Toggle Management

### 9.6. Documentation
*   **Total Estimated Effort**: ~0.5 day
*   **Tasks**:
    *   `[S]` **Task 13**: Create Postman Collection
    *   `[S]` **Task 14**: Add Troubleshooting Section to README
    *   `[S]` **Task 21**: Document Architectural Trade-offs
