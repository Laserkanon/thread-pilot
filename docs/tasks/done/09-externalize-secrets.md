### Task: Externalize Secrets from Version Control

-   **Priority**: Critical
-   **Complexity**: Medium
-   **Description**: A major security vulnerability in the current repository is the presence of hardcoded secrets (database passwords, connection strings) in configuration files and CI scripts. These secrets must be removed from version control and managed through secure means. This task covers updating the local development, Docker Compose, and CI environments to handle secrets safely.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/appsettings.json`
    -   `src/Vehicle/Vehicle.Service/appsettings.json`
    -   `docker-compose.yml`
    -   `.github/workflows/ci.yml`
    -   `.gitignore`

-   **Action Points**:

    **1. Local Development (non-Docker)**
    1.  **Use .NET Secret Manager**: For each service (`Insurance.Service`, `Vehicle.Service`), initialize the .NET Secret Manager: `dotnet user-secrets init`.
    2.  **Store Secrets**: Move the `ConnectionStrings:Default` value from `appsettings.Development.json` into the secret store for each project: `dotnet user-secrets set "ConnectionStrings:Default" "your_local_connection_string"`.
    3.  **Remove from Config**: Delete the `ConnectionStrings` section from all `appsettings.*.json` files. The application will automatically use the user secrets in development mode.

    **2. Docker Compose Environment**
    1.  **Create `.env` file**: Create a file named `.env` in the root directory. This file will hold the secrets for the Docker environment.
        ```dotenv
        # .env file
        DB_SA_PASSWORD=YourStrong!Passw0rd
        ```
    2.  **Update `docker-compose.yml`**: Modify `docker-compose.yml` to use variables from the `.env` file. Replace all hardcoded passwords and connection string components with these variables (e.g., `${DB_SA_PASSWORD}`).
        ```yaml
        services:
          sql-server:
            environment:
              - ACCEPT_EULA=Y
              - SA_PASSWORD=${DB_SA_PASSWORD}
          insurance-service:
            environment:
              - 'ConnectionStrings__Default=Server=sql-server;Database=InsuranceDb;User Id=sa;Password=${DB_SA_PASSWORD};TrustServerCertificate=True;'
            # ... and so on for other services
        ```
    3.  **Update `.gitignore`**: Add `.env` to the `.gitignore` file to ensure it's never committed.
    4.  **Create Template File**: Create a file named `.env.template` that shows the required variables but with placeholder values. Commit this file to the repository so other developers know what secrets to create.

    **3. CI/CD Environment**
    1.  **Use GitHub Actions Secrets**: In the GitHub repository settings, go to `Settings > Secrets and variables > Actions`. Create a new repository secret (e.g., `SA_PASSWORD`) with the password used for CI tests.
    2.  **Update `ci.yml`**: In `.github/workflows/ci.yml`, replace all hardcoded passwords with the GitHub secret.
        ```yaml
        services:
          mssql:
            env:
              ACCEPT_EULA: Y
              SA_PASSWORD: "${{ secrets.SA_PASSWORD }}"
        # ...
        run: dotnet run -- ... --connection "Server=localhost...Password=${{ secrets.SA_PASSWORD }}..."
        ```

-   **Verification**:
    -   After the changes, the application should still run correctly locally using user secrets.
    -   The `docker-compose up` command should work correctly by sourcing secrets from the local `.env` file.
    -   The CI pipeline should pass, successfully connecting to the database using the GitHub Actions secret.
    -   A search for the password string in the codebase should yield no results in any committed files.
