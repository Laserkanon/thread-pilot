# ThreadPilot Integration Layer

This repository contains the solution for the ThreadPilot integration layer assignment. It consists of two microservices, two databases, and a suite of tests, designed to demonstrate a clean, maintainable, and extensible architecture for integrating with legacy systems.

## 1. Architecture and Design

The solution follows a distributed, service-oriented architecture, with each service having its own distinct responsibility and database. This promotes separation of concerns, independent deployment, and scalability.

-   **`Vehicle.Service`**: A microservice responsible for providing vehicle information. It has a single-table database (`VehicleDb`) and exposes a REST API for retrieving vehicle data.
-   **`Insurance.Service`**: A microservice responsible for providing insurance information for a given person. It integrates with `Vehicle.Service` to enrich car insurance policies with vehicle details. It has its own database (`InsuranceDb`).
-   **Database Migrations**: Two console applications, `Vehicle.Db` and `Insurance.Db`, use the **DbUp** library to manage database schema and seed data. This ensures that database changes are version-controlled, repeatable, and easy to apply in any environment.
-   **Data Access**: The repositories use **Dapper**, a high-performance and lightweight micro-ORM, for database queries. This avoids the overhead of a full ORM like Entity Framework while still providing a clean way to map query results to C# objects.
-   **Service Layer**: The core business logic in `Insurance.Service` is handled by an `InsuranceService` class. This class is responsible for fetching data from its own repository and then calling the `Vehicle.Service` to enrich the data. This keeps the controller thin and the logic testable. A key performance consideration here is that it makes **one single batch call** to `Vehicle.Service` for all car insurances, avoiding the N+1 query problem.

### Visual Diagrams

The architecture of each service is visualized in the PlantUML diagrams below.

-   [Vehicle Service Diagram](docs/vehicle_service.puml)
-   [Insurance Service Diagram](docs/insurance_service.puml)

## 2. Running and Testing the Solution Locally

### Prerequisites

-   .NET 8 SDK
-   SQL Server (LocalDB is sufficient)
-   A REST client like `curl` or Postman.

### Steps to Run

1.  **Clone the repository.**
2.  **Set up the databases:**
    Open a terminal in the root of the project and run the DbUp console applications. This will create the databases and apply the latest schema and seed data.
    ```bash
    # Update connection strings in src/Vehicle.Db/appsettings.json and src/Insurance.Db/appsettings.json if needed.

    dotnet run --project src/Vehicle.Db
    dotnet run --project src/Insurance.Db
    ```
3.  **Start the services:**
    Open two separate terminals.
    ```bash
    # In terminal 1
    dotnet run --project src/Vehicle.Service

    # In terminal 2
    dotnet run --project src/Insurance.Service
    ```
    The services will be available at:
    -   `Vehicle.Service`: `http://localhost:5081`
    -   `Insurance.Service`: `http://localhost:5082`

4.  **Verify the endpoints:**
    ```bash
    # Get vehicle details
    curl http://localhost:5081/api/v1/vehicles/ABC123

    # Get insurances for a person (which in turn calls the vehicle service)
    curl http://localhost:5082/api/v1/insurances/199001011234
    ```

### How to Run Tests

Run all unit and integration tests from the root directory:
```bash
dotnet test
```

## 3. Technical Approach

### Error Handling

-   **Invalid Input**: API inputs are validated using **FluentValidation**. If validation fails, the API returns a `400 Bad Request` with a list of validation errors. This is handled in the controller for the insurance service and via middleware for the vehicle service's batch endpoint.
-   **Missing Data**: If a requested resource (like a vehicle or a person's insurances) is not found, the API gracefully returns a `404 Not Found` status code. The orchestrator is designed to handle cases where a vehicle for a car insurance policy is not found in the `Vehicle.Service`, simply leaving the `VehicleDetails` property as null.
-   **Service Unavailability**: The `VehicleServiceClient` uses `HttpClient`, which will throw an `HttpRequestException` if the `Vehicle.Service` is unavailable. In a production scenario, this would be caught by a global exception handler middleware to return a `503 Service Unavailable` response.

### Extensibility

-   **Service-Oriented Architecture**: New services can be added to the solution without impacting existing ones.
-   **Repository Pattern**: The use of repositories abstracts the data access logic. If the underlying database technology were to change, only the repository implementation would need to be updated.
-   **API Versioning**: The APIs are versioned from the start (`/api/v1/...`). This allows for future changes and new versions of the API to be introduced without breaking existing clients.

### Security

-   **Connection Strings**: Connection strings are stored in `appsettings.json` and should be moved to a secure secret management system (like Azure Key Vault or HashiCorp Vault) in a production environment.
-   **Input Validation**: All API inputs are validated to prevent common vulnerabilities like injection attacks and oversized payloads.
-   **HTTPS**: The services are configured to use HTTPS redirection.

## 4. Onboarding and CI/CD

### Onboarding

To enable other developers to work on this solution, they would need to:
1.  Read this `README.md` to understand the architecture and setup.
2.  Ensure they have the prerequisites installed (.NET 8 SDK, SQL Server).
3.  Run the database projects first to set up their local databases.
4.  Run the services and tests as described above.
The solution is structured cleanly into `src` and `tests` directories, with each project having a clear responsibility, making it easy to navigate.

### CI/CD Pipeline

A basic CI pipeline is defined in `.github/workflows/ci.yml`. This GitHub Actions workflow will:
1.  Trigger on every push or pull request.
2.  Set up a SQL Server instance using a service container.
3.  Build the solution.
4.  Run the DbUp projects to apply migrations to the test database.
5.  Run all the unit and integration tests.

## 5. Personal Reflection

I have experience building distributed systems with .NET, often using a similar microservices approach with dedicated databases. The most interesting challenge in this assignment was adhering to the strict-minimal requirements while still building something robust. It forced me to make deliberate choices, like using Dapper for performance and simplicity over EF Core, and implementing a manual validation call in the controller to accommodate primitive type validation with FluentValidation. If I had more time, I would add a global exception handling middleware, implement distributed tracing for observability between the services, and use Testcontainers to spin up a real SQL Server instance for the integration tests to make them even more reliable.