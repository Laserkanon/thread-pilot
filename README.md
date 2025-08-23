# ThreadPilot Integration Layer

This repository contains the solution for the ThreadPilot integration layer assignment. The project demonstrates a robust, maintainable, and scalable approach to integrating a new core system with legacy systems using a microservice architecture in .NET.

It consists of two primary services:
-   **`Vehicle.Service`**: Manages and provides data about vehicles.
-   **`Insurance.Service`**: Manages insurance policies and integrates with `Vehicle.Service` to provide enriched data.

This document provides a comprehensive overview of the architecture, setup instructions, and key design decisions.

---

## 1. Architecture and Design Decisions

The solution is built using a distributed microservice architecture to ensure a clear separation of concerns, independent scalability, and deployment flexibility.

### 1.1. Microservice Architecture

-   **`Vehicle.Service`**: A focused microservice that exposes a REST API for vehicle data. It has its own dedicated database, managed by **DbUp**.
-   **`Insurance.Service`**: A microservice that provides insurance information for individuals. It performs internal orchestration by calling `Vehicle.Service` to enrich car insurance policies with vehicle details.

##  1.2. Vehicle Data Enrichment Strategy
Our primary architectural assumption is that the downstream Vehicle.Service is a legacy system. As such, its load capacity is unknown, and we cannot assume it can be easily extended. Our entire data enrichment strategy is therefore built defensively to respect these constraints.

The default and safest method for fetching vehicle data is to make concurrent, distinct calls to the GetVehicleAsync(registrationNumber) endpoint. This approach avoids placing sequential calls that would have a risk of timing out or be too slow, while also limiting the vehicle service to only look up what it really must by using distinct registration numbers. To prevent overwhelming the Vehicle.Service, the level of concurrency is strictly controlled by the MaxDegreeOfParallelism configuration value, which acts as a vital safety valve. As we mentioned, the vehicle service's performance is unknown, and we placed a configurable parameter with a value of 5 concurrent requests as a starting point, but it can be easily configured depending on the load. Furthermore, all HTTP communication is wrapped in resilience policies, such as Retry and Circuit Breaker, to handle transient network errors and service unavailability gracefully.

However, it is unclear how difficult it would be to change the legacy system. We imagined that if a developer on that team could expose a more efficient endpoint, let's say a batch variant, we would prefer to use it, as it would significantly reduce the overhead of multiple HTTP requests and also increase database efficiency (single vs. multiple lookups). To demonstrate this pattern, we have implemented a client method (GetVehiclesBatchAsync) that could consume such an endpoint.

To provide flexibility and safety, this entire process is controlled by two distinct feature toggles:

EnableVehicleEnrichment: This acts as a global "kill-switch." If the enrichment process causes any issues in production, this toggle can be set to false to disable all calls to the Vehicle.Service entirely.
EnableBatchVehicleCall: This allows an operator to switch between the two strategies. By default, it is false, using the safe, concurrent single-call method. If the Vehicle.Service is ever updated with a trusted batch endpoint, this can be switched to true to gain the performance benefits.

### 1.3. Dependency Injection (DI)

The project uses the standard, built-in .NET dependency injection container. The setup is clean, straightforward, and configured directly in `Program.cs`, following modern .NET best practices. There are no custom or "old-fashioned" DI frameworks or installers.

*Example from `Insurance.Service/Program.cs`:*
```csharp
// Domain services
builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();

// HTTP Client for Vehicle Service
builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>();
```

### 1.4. Data Access

**Dapper** was chosen as the micro-ORM for data access. It offers high performance and a lightweight abstraction over raw ADO.NET without the complexity of a full ORM like Entity Framework, which was deemed unnecessary for this project's scope. Database migrations are handled by **DbUp**, ensuring version-controlled and repeatable schema changes.

### 1.5. Input Validation

**FluentValidation** is used for validating API inputs. This choice promotes a clean and unified approach to validation logic, separating it from the core business logic of the controllers and services. It provides a robust way to define complex validation rules and results in a clear separation of concerns.

### 1.6. Shared Contracts

The solution uses dedicated `.Contracts` projects (e.g., `Insurance.Service.Contracts`) to define the public data models (Data Transfer Objects or DTOs) that are shared between services. This is a critical architectural pattern for several reasons:

-   **Explicit Public Interface**: It creates a clear separation between the service's internal data models (how data is stored in the database) and its public contract (how data is exposed to the outside world). This prevents accidentally exposing internal fields or implementation details, which is important for both security and maintainability.
-   **Improved Developer Experience (DX)**: When a developer needs to change an internal data model, the compiler will force them to consider how that change impacts the public contract, as the mapping will need to be updated. This makes refactoring safer and prevents unintended breaking changes.
-   **Service Decoupling**: A service (like `Insurance.Service`) can consume the contract of another service (`Vehicle.Service.Contracts`) without needing a dependency on its full implementation, reducing coupling and improving build times.
-   **Clear Versioning**: By having the API contract defined in a separate assembly, it becomes easier to manage versioning and support multiple versions of a contract simultaneously in the future.

### 1.7. Authentication and Authorization

The services use a straightforward and secure **API Key-based authentication** mechanism. This approach was chosen for its simplicity and effectiveness in server-to-server communication, which is the primary use case for this integration layer.

-   **How it Works**: Each service is configured with a secret API key. Clients (including other services within the solution) must include this key in the `X-Api-Key` header of their HTTP requests. A custom authentication handler (`ApiKeyAuthHandler`) validates the key. If the key is missing or invalid, the request is rejected with a `401 Unauthorized` response.
-   **Configuration**: The API key for each service is defined in `secrets.local.json` and applied via the `init.ps1` script, which stores them in .NET User Secrets. This ensures that keys are not hardcoded and are managed securely during local development. The `Vehicle.Service` client within the `Insurance.Service` is also configured with the correct key via dependency injection.
-   **Security**: This strategy provides a solid layer of security for internal services, ensuring that only authorized clients can access the APIs. All endpoints across all services are protected by this scheme by default.

This pragmatic approach was chosen for its simplicity. It is currently unclear whether this service will be exposed directly to end-users or will only be used for internal, system-to-system communication. This ambiguity must be resolved before committing to a more sophisticated, user-facing authentication solution like JWT, as a thorough investigation into the service's definitive role could save significant implementation time.

### 1.8. Feature Toggles

The solution uses a simple feature toggle system to enable or disable certain functionality at runtime without requiring a redeployment. The implementation is based on the following principles:

-   **Configuration-based**: Toggles are defined in `appsettings.json` under the `FeatureToggles` section.
-   **Service-based**: A dedicated `FeatureToggleService` abstracts the logic of reading toggle values. This service is injected into other services that need to check a feature's status.
-   **Static Toggles**: The current implementation uses `IConfiguration` to read the toggle values at application startup. This means that any changes to the toggles require an application restart to take effect.

---

## 2. How to Run and Test Locally

### Prerequisites

-   .NET 8 SDK
-   PowerShell
-   Docker and Docker Compose (if using the Docker-based setup)

This project provides two ways to run the services locally: via Docker Compose (recommended for a simple, all-in-one setup) or by running the .NET services directly on your machine with HTTPS.

### Secret Management

This project uses template files (`.env.template`, `secrets.local.json.template`) as a blueprint for the actual secret files (`.env`, `secrets.local.json`) that you will create locally. These template files are checked into source control to show other developers what secrets are needed, while the actual secret files are listed in `.gitignore` and should never be committed.

Before you run the application, you need to configure the necessary secrets (like the database password and API keys). You only need to provide your secrets once, and the `init.ps1` script will automatically configure them for both the Docker and non-Docker environments.

1.  **Create the secrets file**: In the root of the repository, copy the template file `secrets.local.json.template` to a new file named `secrets.local.json`.

2.  **Configure secrets**: Open `secrets.local.json` and provide your secrets. The default template already contains secure, randomly-generated keys that you can use for local development.
    ```json
    {
      "Insurance.Service": {
        "DB_SA_PASSWORD": "YourActualPassword!123",
        "Authentication": {
          "ApiKey": "d3a8b273-5a41-47c3-9a70-7981b1c3a6e8"
        },
        "Vehicle.Service.Client":{
          "ApiKey": "d3a8b273-5a41-47c3-9a70-7981b1c3a6e8"
        }
      },
      "Vehicle.Service": {
        "DB_SA_PASSWORD": "YourActualPassword!123",
        "Authentication": {
          "ApiKey": "d3a8b273-5a41-47c3-9a70-7981b1c3a6e8"
        }
      },
      "Insurance.Db": {
        "DB_SA_PASSWORD": "YourActualPassword!123"
      },
      "Vehicle.Db": {
        "DB_SA_PASSWORD": "YourActualPassword!123"
      }
    }
    ```
    The `init.ps1` script will use these values to configure the services.

3.  **Run the configuration script**: Open a terminal in the root of the repository and run the PowerShell script.
    ```powershell
    ./init.ps1
    ```
    This script will perform two actions:
    -   It will create a `.env` file in the root directory. This file is used by Docker Compose.
    -   It will use the .NET Secret Manager to configure secrets for local development (when not using Docker).

After completing these steps, you are ready to run the application using either of the options below.

### Option A: Docker Compose

This is the simplest way to get the entire solution running, as it handles database setup and service configuration automatically. This setup uses plain HTTP for simplicity in local development.

1.  **Ensure Docker Desktop is running.**
2.  **Run Docker Compose:** From the root directory of the project, run:
    ```bash
    docker-compose -f docker-compose.local.yml up --build
    ```
    This command will use the `docker-compose.local.yml` file and the `.env` file you generated to:
    -   Build the Docker images for each service.
    -   Start containers for the `Vehicle.Service`, `Insurance.Service`, and a shared SQL Server database.
    -   Automatically run database migrations to set up the required schemas and seed initial data.

#### Verifying the Services with Docker

The services are exposed on the following HTTP ports:
-   **`Vehicle.Service`**: `http://localhost:5081`
-   **`Insurance.Service`**: `http://localhost:5082`

You can use the interactive Swagger UI or `curl` to test the endpoints:
-   **Vehicle Service Swagger UI**: [http://localhost:5081/swagger](http://localhost:5081/swagger)
-   **Insurance Service Swagger UI**: [http://localhost:5082/swagger](http://localhost:5082/swagger)

### Option B: Manual Setup

If you prefer to run the services directly on your machine:

1.  **Trust the .NET Development Certificate**: Run the following command once to set up and trust the local HTTPS development certificate.
    ```bash
    dotnet dev-certs https --trust
    ```
2.  **Set up a local database** You can run this to create a mssql container: `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourActualPassword!123" -p 1433:1433 --name sqlserver  -d mcr.microsoft.com/mssql/server:2022-latest` or use a local SQL Server instance. Don't forget to provide the `secrets.local.json` password. 

2.1 **Container**: 


3.  **Run Database Migrations**:
    The `init.ps1` script has already set up the secrets for your local .NET environment. However, the database migration tools are simple console apps that don't use the full application host, so we need to pass the connection string directly.
    ```bash
    # Apply Vehicle DB Migrations
    dotnet run --project src/Vehicle/Vehicle.Db -- --connection "Server=localhost,1433;Database=VehicleDb;User Id=sa;Password=Passw0rd!123;TrustServerCertificate=True"

    # Apply Insurance DB Migrations
    dotnet run --project src/Insurance/Insurance.Db -- --connection "Server=localhost,1433;Database=InsuranceDb;User Id=sa;Password=Passw0rd!123;TrustServerCertificate=True"
    ```
    *(Remember to replace `Passw0rd!123` with your actual password if you changed it in `secrets.local.json`)*.

4.  **Start the services**:
    Open two separate terminals and run the following commands. The services will use the secrets you configured and will run on HTTPS.
    ```bash
    # In terminal 1: Start the Vehicle Service
    dotnet run --project src/Vehicle/Vehicle.Service
    # Service will be available at https://localhost:7297

    # In terminal 2: Start the Insurance Service
    dotnet run --project src/Insurance/Insurance.Service
    # Service will be available at https://localhost:7296
    ```

#### Verifying the Endpoints (Manual Setup)

When you run the services locally, a Swagger UI page should automatically open in your browser at the HTTPS endpoints:
```
- Vehicle Service: https://localhost:7297/swagger
- Insurance Service: https://localhost:7296/swagger
```

### Running Tests

You can run all unit and integration tests for the entire solution by executing the following command from the root directory:
```bash
dotnet test
```
Alternatively, you can target a specific test project to run its tests in isolation (e.g., `dotnet test src/Insurance/Insurance.UnitTests`).

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

### Testing Strategy

The solution includes both unit and integration tests to ensure correctness and stability.
-   **Unit Tests**: These focus on testing individual components in isolation. For example, the `InsuranceService` unit tests mock both the database repository and the `IVehicleServiceClient` to verify the service's logic without making real HTTP calls or database queries. This is a standard practice to ensure tests are fast, focused, and completely isolated from external dependencies.
-   **Integration Tests**: These tests validate the interaction between Insurance.Service and its direct dependencies. They start up the service together with its test database, then perform a real HTTP call from Insurance.Service through to the database and back, ensuring the full end-to-end data flow works as expected. External microservice dependencies are mocked. While mocking isn’t strictly necessary, the author suggests introducing an additional, broader test project at a third level to keep these tests focused and fast, while still allowing more comprehensive scenarios to be verified separately without slowing down the core integration suite.

### Error Handling

-   **Invalid Input (`400 Bad Request`)**: API inputs are validated using **FluentValidation**. If validation fails, the API returns a `400` response with clear error messages.
-   **Missing Data (`404 Not Found`)**: If a resource like a vehicle or a person's insurance profile is not found, the API returns a `404`.
-   **Graceful Degradation**: If the `Vehicle.Service` is called for a specific car insurance policy but does not find a matching vehicle, it returns `null` for that vehicle's details. This is a **deliberate design choice** to ensure that the entire request for a person's insurances doesn't fail just because one vehicle record is missing. The system gracefully degrades rather than failing completely.

### Extensibility and API Versioning

-   The service-oriented architecture allows new services to be added with minimal impact on existing ones.
-   The use of repository and service abstractions makes it easier to modify or replace implementations (e.g., swapping out the database).
-   All APIs are versioned from the start (`/api/v1/...`) to allow for future, non-breaking changes.

### Observability

The solution includes basic observability features out of the box.

-   **Health Checks**: Both services expose a standard health check endpoint at `/healthz`. This can be used by container orchestration systems like Kubernetes or service meshes to automatically manage application health.
-   **Application Metrics**: Both services use **OpenTelemetry** to expose a wide range of application metrics in a **Prometheus**-compatible format. This includes standard metrics for HTTP requests (e.g., duration, count, status codes) and HttpClient calls. The metrics are available on the `/metrics` endpoint of each service and can be scraped by a Prometheus server for monitoring and alerting.
-   **Structured Logging**: The services are configured to use **Serilog** for structured logging. Logs are written to the console in a machine-readable JSON format, which makes them easy to collect, parse, and analyze in a centralized logging platform.

### Resilience

The `Insurance.Service` implements key resilience patterns using **Polly** for its calls to the `Vehicle.Service`. This makes the system more robust against transient failures and service unavailability. The following policies are in place:
-   **Retry Policy**: Automatically retries failed HTTP requests with an exponential backoff strategy to handle temporary network issues or service hiccups.
-   **Circuit Breaker**: Prevents the service from repeatedly calling a known-to-be-unhealthy `Vehicle.Service`. After a configurable number of consecutive failures, the circuit "opens," and subsequent calls fail immediately for a set duration, allowing the downstream service time to recover.
-   **Fallback Policy**: When the circuit is open, a fallback mechanism provides a default response (an empty list of vehicles) instead of throwing an exception. This ensures that the `Insurance.Service` can still function gracefully and provide a partial response to its clients even when its dependency is down.

---

## 4. CI/CD and Developer Onboarding

### CI/CD Pipeline

A basic Continuous Integration (CI) pipeline is defined in `.github/workflows/ci.yml`. This GitHub Actions workflow triggers on every push and pull request to automatically build the solution, run the tests, and ensure the codebase remains in a healthy state. On pushes to any branch, it also publishes NuGet packages for the service contracts and Docker images for the services to the GitHub Package Registry, using a dynamic versioning strategy for feature branches.

### Onboarding New Developers

A new developer can get started by:
1.  Cloning the repository.
2.  Ensuring prerequisites are installed (.NET 8, Docker).
3.  Reading this `README.md` to understand the project structure and design.
4.  Running `docker-compose -f docker-compose.local.yml up --build` to get a fully working local environment in a single step.

### AI-Assisted Development

Most of the code in this solution has been written by AI assistants, including ChatGPT-5 and the agent Google Jules. The developer's role was focused on performing critical reviews, making architectural decisions, and identifying and fixing subtle issues that the AI was not able to see. The developer also performed manual programming when required.

A good example of a necessary human correction was in solving the N+1 problem when calling the Vehicle service. An initial AI-generated solution proposed fetching vehicle data for each registration number individually and also missed applying a `Distinct()` operation on the list of registration numbers. While the speed and parallel work provided by AI were beneficial, human oversight was crucial for ensuring the final implementation was both correct and performant.

---

## 5. Personal Reflection

Any similar project or experience you’ve had in the past:
Yes, I recognize the issues in this assignment and the complexity they bring. It’s quite rare to get the opportunity to build everything from scratch, which really highlights the importance of strong platform libraries and how much they can accelerate development. (When you see whats left out)

What was challenging or interesting in this assignment:
I have experience building services with REST call dependencies in critical systems. One challenge is fetching batch-related data from a service—it’s easy to overlook potential issues. A common mitigation is denormalizing data over a queue and storing related data locally, though this introduces additional complexity. Another approach can be for the the client poll/stream instead of keeping the HttpClient waiting, to let the server getg a chance to prepare the data.

What you would improve or extend if you had more time:
I would focus on writing more tests. I’d also prioritize setting up a CI/CD pipeline for publishing Docker images and related NuGet packages as the next steps. Beyond that, I’ve left a non-complete list of further improvements to consider.

---

## 6. Future Improvements

This section outlines potential enhancements to the solution, categorized for clarity.

**NOTE: this list is not final and there are many additional things to consider.**

### Observability and Resilience
-   **Distributed Tracing**: Integrate a distributed tracing solution like OpenTelemetry to provide end-to-end visibility of requests as they travel across services, making it easier to diagnose latency and errors.
-   **Future Resilience Enhancements**: Further resilience patterns like **Bulkhead** isolation and **Rate Limiting** could be added to provide even greater protection against cascading failures.

### Architecture and Design
-   **Event-Driven Architecture**: Explore evolving the architecture to incorporate asynchronous messaging (e.g., with RabbitMQ or Kafka). This would decouple services further and enable patterns like **CQRS** and **Sagas** for more complex workflows.
-   **Native AOT Compilation**: Investigate compiling the services to **Native AOT** (Ahead-of-Time) to significantly reduce memory footprint and startup times, making the services more efficient and scalable, especially in containerized environments.
-   **API Client Generation**: Automate the creation of the `VehicleServiceClient` by generating it directly from the `Vehicle.Service`'s OpenAPI/Swagger specification. This ensures the client is always in sync with the API contract.
-   **Enhanced API Documentation**: Improve the existing Swagger documentation by adding detailed XML comments (`<summary>`, `<param>`, `<returns>`) to the API controllers and models. This would provide clearer, auto-generated guidance for API consumers.
-   **Dynamic Feature Toggles**: Refactor the feature toggle implementation to use `IOptionsMonitor` instead of `IConfiguration`. This would allow for dynamic reloading of feature toggles from `appsettings.json` without an application restart. For a more advanced setup, integrate a centralized feature toggle management service like LaunchDarkly or Azure App Configuration for real-time control over features in production.

### Security
-   **Centralized Secret Management**: Refactor the solution to remove secrets (like database passwords) from `appsettings.json` and `docker-compose.yml` files. Instead, integrate a proper secret management tool like HashiCorp Vault or Azure Key Vault. This addresses the concern of having passwords committed to the repository and prepares the application for secure production deployments.
-   **Secret Scanning**: Integrate automated secret scanning tools (like Gitleaks) into the CI pipeline to prevent sensitive information like API keys or credentials from being accidentally committed to the repository.

### Testing and Quality Assurance
-   **Contract Testing**: Formalize the API contracts between services using a framework like Pact. This would ensure that changes to `Vehicle.Service` do not break `Insurance.Service`'s expectations, catching integration issues early in the development cycle.
-   **Code Quality and Coverage**: Integrate **SonarCloud** or a similar static analysis tool into the CI pipeline to enforce code quality standards and track test coverage over time. *(Note: The CI pipeline already generates a coverage report, but no quality gates are enforced.)*
-   **Infrastructure as Code (IaC)**: Use a tool like **Terraform** to define and manage the cloud infrastructure required for staging and production environments, ensuring consistency and repeatability.
-   **Containerized Integration Tests**: Use **Testcontainers** to spin up ephemeral database instances for integration tests, further isolating them from the local development environment.
-   **Full Environment Regression Tests**: Create a dedicated test suite that uses Docker Compose to run all services together. This would enable testing of the entire system in a simulated full environment, providing the highest level of confidence that the services interact correctly without mocks.

### CI/CD and Deployment
-   **Advanced Deployment Strategies**: Enhance the CI/CD pipeline to support advanced deployment strategies like **Blue-Green deployments** or **Canary Releases**. This would minimize downtime and risk during production releases.

### Repository structure

- **Consider mono vs seperation by bounded service**

---

## 7. Task Estimates and Future Work

The following is a high-level, categorized list of the pending tasks documented in the `docs/tasks` directory. This list can be used for prioritization and planning.

**Legend**:
*   **S (Small)**: ~1-2 hours
*   **M (Medium)**: ~2-4 hours
*   **L (Large)**: ~4-8 hours
*   **XL (Extra Large)**: > 8 hours

### 7.1. Security (High Priority)
*   **Total Estimated Effort**: ~1.5 days
*   **Tasks**:
    *   `[S]` **Task 30**: Harden API Key Authentication
    *   `[M]` **Task 40**: Implement API Quotas and Rate Limiting
    *   `[M]` **Task 46**: Integrate Azure Key Vault
    *   `[M]` **Task 53**: Integrate Secret Scanning

### 7.2. Architecture & Refactoring (Improves Code Quality)
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

### 7.3. Testing & Quality Assurance (Improves Reliability)
*   **Total Estimated Effort**: ~5-6 days
*   **Tasks**:
    *   `[M]` **Task 42**: Implement OpenAPI Snapshot Testing
    *   `[L]` **Task 06**: Add Load and Performance Tests
    *   `[L]` **Task 44**: Extend Integration Test Infrastructure
    *   `[L]` **Task 49**: Add Full Environment Regression Test Suite
    *   `[L]` **Task 54**: Implement Containerized Integration Tests (Testcontainers)

### 7.4. CI/CD & DevOps (Improves Automation)
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

### 7.5. New Features & Observability
*   **Total Estimated Effort**: ~2-3 days
*   **Tasks**:
    *   `[S]` **Task 02**: Add Metrics for Feature Toggles
    *   `[S]` **Task 51**: Implement Dynamic Feature Toggles
    *   `[M]` **Task 18**: Add Distributed Tracing
    *   `[M]` **Task 50**: Implement the Bulkhead Resilience Pattern

### 7.6. Documentation
*   **Total Estimated Effort**: ~0.5 day
*   **Tasks**:
    *   `[S]` **Task 13**: Create Postman Collection
    *   `[S]` **Task 14**: Add Troubleshooting Section to README
    *   `[S]` **Task 21**: Document Architectural Trade-offs
