### Task: Externalize SQL Username from Configuration

-   **Priority**: High
-   **Complexity**: Small
-   **Description**: Currently, the SQL Server username (`sa`) is hardcoded in the application's configuration builders. While the password has been externalized, the username should also be treated as a secret to avoid leaking infrastructure details and to provide flexibility for different environments (e.g., using a non-admin user in production). This task involves updating the configuration, secrets management, and CI/CD pipeline to handle the SQL username as a secret.
-   **Affected Files**:
    -   `secrets.local.json.template`
    -   `init.ps1`
    -   `docker-compose.local.yml`
    -   `src/Infrastructure/Hosting/ConfigurationHostingExtensions.cs` (or similar)
    -   `.github/workflows/ci.yml`

-   **Action Points**:

    **1. Update Secret Templates and Scripts**
    1.  **Update `secrets.local.json.template`**: Add a new key for the SQL username (e.g., `DB_USER`).
        ```json
        {
          "Insurance.Service": {
            "DB_USER": "sa",
            "DB_SA_PASSWORD": "YourStrongPassword!123",
            // ... other secrets
          }
          // ... other services
        }
        ```
    2.  **Update `init.ps1`**: Modify the script to read the new `DB_USER` secret from `secrets.local.json` and set it in both the `.env` file (for Docker) and the .NET User Secrets store (for local development).

    **2. Update Application Configuration**
    1.  **Update `docker-compose.local.yml`**: Modify the service definitions to use the new `DB_USER` environment variable when constructing connection strings.
        ```yaml
        services:
          insurance-service:
            environment:
              - 'ConnectionStrings__Default=Server=sql-server;Database=InsuranceDb;User Id=${DB_USER};Password=${DB_SA_PASSWORD};TrustServerCertificate=True;'
            # ... and so on for other services
        ```
    2.  **Update Connection String Building**: Locate the code where the connection string is constructed in the application (e.g., in `ConfigurationHostingExtensions.cs`) and modify it to read the username from the configuration, similar to how the password is read.

    **3. Update CI/CD Environment**
    1.  **Add GitHub Actions Secret**: In the repository settings, add a new GitHub Actions secret for the SQL username (e.g., `DB_USER`) used in the CI environment.
    2.  **Update `ci.yml`**: Modify the CI workflow to use the new `DB_USER` secret when setting up the database service and running tests.

-   **Verification**:
    -   The application must run correctly both locally (via `dotnet run`) and with Docker Compose, connecting to the database using the externalized username.
    -   The CI pipeline must pass, using the new secret for the database username.
    -   A search for `User Id=sa` or similar hardcoded usernames in the codebase should yield no results in any committed configuration or source code files.
