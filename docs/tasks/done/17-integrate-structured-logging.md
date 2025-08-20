### Task: Integrate Structured Logging with Serilog

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: The current logging implementation uses the default .NET logger, which produces plain-text messages that are difficult to query and analyze. To improve observability, this task involves integrating a structured logging library, [Serilog](https://serilog.net/), to produce machine-readable JSON logs. This is a foundational step for effective log management in a distributed system.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Program.cs`
    -   `src/Vehicle/Vehicle.Service/Program.cs`
    -   `src/Insurance/Insurance.Service/appsettings.json`
    -   `src/Vehicle/Vehicle.Service/appsettings.json`
    -   `src/Insurance/Insurance.Service/Insurance.Service.csproj`
    -   `src/Vehicle/Vehicle.Service/Vehicle.Service.csproj`

-   **Action Points**:

    1.  **Add Serilog NuGet Packages**: In the `.csproj` files for both `Insurance.Service` and `Vehicle.Service`, add the following Serilog packages:
        -   `Serilog.AspNetCore`: The main integration package for ASP.NET Core.
        -   `Serilog.Sinks.Console`: To write logs to the console.
        -   `Serilog.Formatting.Json.JsonFormatter`: To format console logs as JSON.
        -   `Serilog.Settings.Configuration`: To read configuration from `appsettings.json`.

    2.  **Configure Serilog in `Program.cs`**: In the `Program.cs` for both services, replace the default logger with Serilog. This is typically done at the very start of the file.
        ```csharp
        // At the top of Program.cs
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog(); // Add this line

        // ... rest of the Program.cs
        ```
        A `try/finally` block around the application startup/run logic is also recommended to ensure any startup errors are logged.

    3.  **Configure Serilog in `appsettings.json`**: Move the logging configuration from the default `Logging` section to a new `Serilog` section in the `appsettings.json` of both services. This allows the logging configuration to be changed without redeploying.
        ```json
        // Remove the "Logging" section and add this:
        "Serilog": {
          "MinimumLevel": {
            "Default": "Information",
            "Override": {
              "Microsoft": "Warning",
              "System": "Warning"
            }
          },
          "WriteTo": [
            {
              "Name": "Console",
              "Args": {
                "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
              }
            }
          ],
          "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
        }
        ```
    4.  **Refactor Example Log Message**: Find an existing log message (e.g., in `VehicleServiceClient.cs`) and refactor it to use structured properties.
        -   **Before**: `_logger.LogWarning("One or more registrations were not found. {registrationNumbers}", JsonSerializer.Serialize(registrationNumbers));`
        -   **After**: `_logger.LogWarning("One or more registrations were not found. RegistrationNumbers: {RegistrationNumbers}", registrationNumbers);`
        By not pre-serializing the object, Serilog can capture it as a structured property in the JSON output, making it easy to search for logs containing a specific registration number.

-   **Verification**:
    -   Run the services via `docker-compose up`.
    -   The console output for both services should now be in JSON format.
    -   Examine a JSON log entry and verify that it contains the structured properties (e.g., `RegistrationNumbers` as an array) and enriched data (e.g., `MachineName`).
