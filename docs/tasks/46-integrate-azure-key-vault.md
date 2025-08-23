### Task: Integrate Azure Key Vault for Secret Management

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: To improve security and manageability for deployed environments, the application should be configured to fetch its secrets from Azure Key Vault instead of relying on environment variables or configuration files. This task involves updating the application to use Managed Identity to connect to Key Vault, while preserving the existing User Secrets-based setup for local development.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Program.cs`
    -   `src/Vehicle/Vehicle.Service/Program.cs`
    -   `src/Insurance/Insurance.Service/Insurance.Service.csproj`
    -   `src/Vehicle/Vehicle.Service/Vehicle.Service.csproj`
-   **Action Points**:
    1.  **Add NuGet Package**: Add the `Azure.Extensions.AspNetCore.Configuration.Secrets` and `Azure.Identity` packages to both service projects.
    2.  **Update `Program.cs`**: In both `Program.cs` files, add logic to register Azure Key Vault as a configuration source when the application is *not* running in a `Development` environment. The Key Vault URI should be read from an environment variable (e.g., `KEY_VAULT_URI`).
        ```csharp
        // In Program.cs, after WebApplication.CreateBuilder(args);
        if (!builder.Environment.IsDevelopment())
        {
            var keyVaultUri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
            if (!string.IsNullOrEmpty(keyVaultUri))
            {
                builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
            }
        }
        ```
    3.  **Infrastructure Setup (Prerequisites)**: This task depends on infrastructure being in place. The following must be configured in the target Azure environment:
        -   An Azure Key Vault instance must be created.
        -   Secrets must be added to the vault (e.g., `ConnectionStrings--Default`). Note the use of `--` to represent the `:` separator from `appsettings.json`.
        -   The deployed application (e.g., App Service, Container App) must have Managed Identity enabled.
        -   The Managed Identity must be granted `Get` and `List` permissions on the Key Vault's secrets.
        -   The `KEY_VAULT_URI` environment variable must be set in the application's deployment configuration.

-   **Verification**:
    -   When running locally (`ASPNETCORE_ENVIRONMENT=Development`), the application should continue to use secrets from User Secrets and `secrets.local.json`.
    -   When deployed to an Azure environment with the correct configuration, the application should start successfully and fetch its secrets from Key Vault, overriding any values in `appsettings.json`. For example, the database connection should use the connection string from Key Vault.
