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

### 1.2. Performance: Solving the N+1 Problem

A critical design consideration was ensuring performant integration between services, especially when fetching vehicle details for multiple car insurances. To avoid the classic "N+1 query" problem, the `InsuranceService` is explicitly designed to be efficient by making a **single, batched API call**:
1.  It retrieves all insurances for a given person.
2.  It gathers all unique `CarRegistrationNumber` values from the policies.
3.  It sends **one** request to `Vehicle.Service` with all registration numbers.
4.  It maps the results back to the corresponding insurance policies in memory.

This efficient approach ensures that even if a person has many car insurances, the system load remains minimal.

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

---

## 2. How to Run and Test Locally

### Prerequisites

-   .NET 8 SDK
-   Docker and Docker Compose

### Option A: Docker Compose (Recommended)

This is the simplest way to get the entire solution running, as it handles database setup and service configuration automatically.

1.  **Clone the repository.**
2.  **Navigate to the root directory** of the project in your terminal.
3.  **Run Docker Compose:**
    ```bash
    docker-compose up --build
    ```
    This command will:
    -   Build the Docker images for each service.
    -   Start containers for the `Vehicle.Service`, `Insurance.Service`, and a shared SQL Server database.
    -   Automatically run database migrations to set up the required schemas and seed initial data.

    You should see logs from all services in your terminal. Wait for the health checks to pass to ensure everything is running correctly.

#### Verifying the Services with Docker

Once the containers are running, you can verify that the services are operational. The services are exposed on the following ports:
-   **`Vehicle.Service`**: `http://localhost:5081`
-   **`Insurance.Service`**: `http://localhost:5082`

You can use the interactive Swagger UI or `curl` to test the endpoints:

-   **Vehicle Service**:
    -   **Swagger UI**: [http://localhost:5081/swagger](http://localhost:5081/swagger)
    -   **Example `curl`**:
        ```bash
        # Get vehicle details for a specific car
        curl http://localhost:5081/api/v1/vehicles/ABC123
        ```

-   **Insurance Service**:
    -   **Swagger UI**: [http://localhost:5082/swagger](http://localhost:5082/swagger)
    -   **Example `curl`**:
        ```bash
        # Get all insurances for a person (which in turn calls the vehicle service)
        curl http://localhost:5082/api/v1/insurances/199001011234
        ```

### Option B: Manual Setup (without Docker)

If you prefer to run the services directly on your machine:

1.  **Clone the repository.**
2.  **Set up the databases:**
    -   Ensure you have a local SQL Server instance running.
    -   Update the connection strings in `src/Vehicle.Db/appsettings.json` and `src/Insurance.Db/appsettings.json`.
    -   Run the DbUp console applications to create and seed the databases:
        ```bash
        dotnet run --project src/Vehicle.Db
        dotnet run --project src/Insurance.Db
        ```
3.  **Start the services:**
    Open two separate terminals and run the following commands. The ports are configured in `launchSettings.json` and should not conflict.
    ```bash
    # In terminal 1: Start the Vehicle Service
    dotnet run --project src/Vehicle.Service
    # Service will be available at http://localhost:5297

    # In terminal 2: Start the Insurance Service
    dotnet run --project src/Insurance.Service
    # Service will be available at http://localhost:5296
    ```

#### Verifying the Endpoints (Manual Setup)

When you run the services locally, a Swagger UI page should automatically open in your browser. This interface allows you to explore and test the API endpoints interactively.

Alternatively, you can use `curl` or a tool like Postman. Note that these examples use the ports for the manual setup (`5297` and `5296`):

```bash
# Get vehicle details for a specific car
curl http://localhost:5297/api/v1/vehicles/ABC123

# Get all insurances for a person (which in turn calls the vehicle service)
curl http://localhost:5296/api/v1/insurances/199001011234
```

### Running Tests

You can run all unit and integration tests for the entire solution by executing the following command from the root directory:
```bash
dotnet test
```
Alternatively, you can target a specific test project to run its tests in isolation (e.g., `dotnet test src/Insurance/Insurance.UnitTests`).

---

## 3. Technical Approach Details

### Testing Strategy

The solution includes both unit and integration tests to ensure correctness and stability.
-   **Unit Tests**: These focus on testing individual components in isolation. For example, the `InsuranceService` unit tests mock both the database repository and the `IVehicleServiceClient` to verify the service's logic without making real HTTP calls or database queries. This is a standard practice to ensure tests are fast, focused, and completely isolated from external dependencies.
-   **Integration Tests**: These tests verify the interaction between the `Insurance.Service` and its direct dependencies. The current suite focuses on the critical integration with `Vehicle.Service`. It launches both services and a real test database, then makes an actual HTTP call from `Insurance.Service` to `Vehicle.Service` to ensure the end-to-end data flow for enrichment is correct. In this scenario, no mocking is used for the core components being tested together.

### Error Handling

-   **Invalid Input (`400 Bad Request`)**: API inputs are validated using **FluentValidation**. If validation fails, the API returns a `400` response with clear error messages.
-   **Missing Data (`404 Not Found`)**: If a resource like a vehicle or a person's insurance profile is not found, the API returns a `404`.
-   **Graceful Degradation**: If the `Vehicle.Service` is called for a specific car insurance policy but does not find a matching vehicle, it returns `null` for that vehicle's details. This is a **deliberate design choice** to ensure that the entire request for a person's insurances doesn't fail just because one vehicle record is missing. The system gracefully degrades rather than failing completely.

### Extensibility and API Versioning

-   The service-oriented architecture allows new services to be added with minimal impact on existing ones.
-   The use of repository and service abstractions makes it easier to modify or replace implementations (e.g., swapping out the database).
-   All APIs are versioned from the start (`/api/v1/...`) to allow for future, non-breaking changes.

---

## 4. CI/CD and Developer Onboarding

### CI/CD Pipeline

A basic Continuous Integration (CI) pipeline is defined in `.github/workflows/ci.yml`. This GitHub Actions workflow triggers on every push and pull request to automatically build the solution, run the tests, and ensure the codebase remains in a healthy state.

### Onboarding New Developers

A new developer can get started by:
1.  Cloning the repository.
2.  Ensuring prerequisites are installed (.NET 8, Docker).
3.  Reading this `README.md` to understand the project structure and design.
4.  Running `docker-compose up --build` to get a fully working local environment in a single step.

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
-   **Structured Logging**: Implement structured logging (e.g., using Serilog) to create consistent, machine-readable log output. This would enable easier integration with log aggregation platforms like the ELK stack or Splunk.
-   **Distributed Tracing**: Integrate a distributed tracing solution like OpenTelemetry to provide end-to-end visibility of requests as they travel across services, making it easier to diagnose latency and errors.
-   **Expanded Resilience Policies**: Enhance the existing resilience strategy by adding more advanced patterns from libraries like **Polly**. This includes implementing **Retry** policies for transient failures, **Circuit Breakers** to prevent cascading failures, and patterns like **Bulkhead** isolation and **Rate Limiting** to protect services from being overwhelmed.
-   **Health Checks and Metrics**: Implement detailed health check endpoints (`/health`) that report the status of downstream dependencies (like databases). Expose application metrics (e.g., request rates, error percentages) in a Prometheus format for monitoring and alerting.

### Architecture and Design
-   **Event-Driven Architecture**: Explore evolving the architecture to incorporate asynchronous messaging (e.g., with RabbitMQ or Kafka). This would decouple services further and enable patterns like **CQRS** and **Sagas** for more complex workflows.
-   **Native AOT Compilation**: Investigate compiling the services to **Native AOT** (Ahead-of-Time) to significantly reduce memory footprint and startup times, making the services more efficient and scalable, especially in containerized environments.
-   **API Client Generation**: Automate the creation of the `VehicleServiceClient` by generating it directly from the `Vehicle.Service`'s OpenAPI/Swagger specification. This ensures the client is always in sync with the API contract.
-   **Feature Toggles**: Introduce a feature toggle library to allow for dynamic enabling or disabling of features in production without requiring a redeployment.

### Security
-   **Authentication and Authorization**: Implement a robust security model, such as **JWT-based authentication**, to secure the APIs and ensure that only authorized clients can access them.
-   **Centralized Secret Management**: Refactor the solution to remove secrets (like database passwords) from `appsettings.json` and `docker-compose.yml` files. Instead, integrate a proper secret management tool like HashiCorp Vault or Azure Key Vault. This addresses the concern of having passwords committed to the repository and prepares the application for secure production deployments.
-   **Secret Scanning**: Integrate automated secret scanning tools (like Gitleaks) into the CI pipeline to prevent sensitive information like API keys or credentials from being accidentally committed to the repository.

### Testing and Quality Assurance
-   **Contract Testing**: Formalize the API contracts between services using a framework like Pact. This would ensure that changes to `Vehicle.Service` do not break `Insurance.Service`'s expectations, catching integration issues early in the development cycle.
-   **Code Quality and Coverage**: Integrate **SonarCloud** or a similar static analysis tool into the CI pipeline to enforce code quality standards and track test coverage over time.
-   **Infrastructure as Code (IaC)**: Use a tool like **Terraform** to define and manage the cloud infrastructure required for staging and production environments, ensuring consistency and repeatability.
-   **Containerized Integration Tests**: Use **Testcontainers** to spin up ephemeral database instances for integration tests, further isolating them from the local development environment.
-   **Automated E2E Regression Testing**: Establish a dedicated suite of end-to-end (E2E) regression tests that run against a fully deployed, production-like environment. This would provide the highest level of confidence that the entire system functions correctly as a whole.

### CI/CD and Deployment
-   **Publish contracts and docker images**
-   **Advanced Deployment Strategies**: Enhance the CI/CD pipeline to support advanced deployment strategies like **Blue-Green deployments** or **Canary Releases**. This would minimize downtime and risk during production releases.

### Repository structure

- **Consider mono vs seperation by bounded service**
