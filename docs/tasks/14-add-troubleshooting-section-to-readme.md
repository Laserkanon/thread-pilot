### Task: Add a Troubleshooting Section to README.md

-   **Priority**: Low
-   **Complexity**: Low
-   **Description**: To help new developers solve common problems independently, a "Troubleshooting" or "Common Issues" section should be added to the main `README.md`. This section will provide quick solutions to frequent issues encountered during setup and local development.
-   **Affected Files**:
    -   `README.md`

-   **Action Points**:

    1.  **Create a New Section**: Add a new top-level section to `README.md` titled "Troubleshooting".
    2.  **Add Content**: Populate the section with a few common problems and their solutions. Use a question-and-answer format.
        -   **Problem 1: Docker Compose fails to start, complaining about ports.**
            -   **Solution**: "This usually means another process on your machine is using one of the ports required by the services (e.g., 5081, 5082, or 1433). Use a tool like `netstat` or `lsof` to find the conflicting process and stop it, or change the port mappings in the `docker-compose.yml` file (e.g., change `\"5082:8080\"` to `\"5083:8080\"`)."
        -   **Problem 2: The Insurance service returns insurances, but `vehicleDetails` is always `null`.**
            -   **Solution**: "This can happen for a few reasons: 1) The `Vehicle.Service` is not running or is unhealthy. Check its logs with `docker-compose logs vehicle-service`. 2) The `FeatureToggles:EnableVehicleEnrichment` flag is set to `false` in `src/Insurance/Insurance.Service/appsettings.json`. 3) The vehicle registration number for the insurance policy does not exist in the vehicle database. The system is designed to gracefully degrade in this case."
        -   **Problem 3: When running manually, I get database connection errors.**
            -   **Solution**: "Ensure you have updated the connection strings in both `src/Insurance.Db/appsettings.json` and `src/Vehicle.Db/appsettings.json` to point to your local SQL Server instance. Also, make sure you have run both DbUp migration projects (`dotnet run --project src/Vehicle.Db` and `dotnet run --project src/Insurance.Db`) to create and seed the databases before starting the services."
    3.  **Review and Refine**: Read through the new section to ensure it is clear, concise, and easy to understand for someone new to the project.

-   **Verification**:
    -   The `README.md` file should be updated with the new "Troubleshooting" section.
    -   The content should be well-formatted and provide helpful advice for the described problems.
