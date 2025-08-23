### Task: Cleanup Project Dependencies and Settings

-   **Priority**: Medium
-   **Complexity**: Low
-   **Description**: The project files (`.csproj`) for both services contain several items that should be cleaned up to improve code hygiene and ensure stability. This includes removing unused packages, updating packages from beta to stable releases, and removing the preview language version setting.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Insurance.Service.csproj`
    -   `src/Vehicle/Vehicle.Service/Vehicle.Service.csproj`
-   **Action Points**:
    1.  **Remove Unused `JwtBearer` Package**: The `Microsoft.AspNetCore.Authentication.JwtBearer` package is referenced in both service projects but is no longer needed since the authentication mechanism was changed to API keys. Remove this `PackageReference`.
    2.  **Update Beta OpenTelemetry Package**: The `OpenTelemetry.Exporter.Prometheus.AspNetCore` package is a beta version (`1.12.0-beta.1`). Check for the latest stable version on NuGet.org and update the `Version` attribute in the `PackageReference`.
    3.  **Remove Preview Language Version**: Both project files contain the `<LangVersion>preview</LangVersion>` tag. This should be removed to default to the latest stable C# language version associated with the .NET 8 SDK.
    4.  **Verification**: After making these changes, rebuild the entire solution (`dotnet build`) to ensure that everything still compiles correctly. Run all tests (`dotnet test`) to verify that the changes have not introduced any regressions.
