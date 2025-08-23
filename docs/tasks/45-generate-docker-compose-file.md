### Task: Dynamically Generate Docker Compose File

-   **Priority**: Low
-   **Complexity**: High
-   **Description**: The `docker-compose.local.yml` file is currently maintained manually. As new services are added to the solution, developers must remember to update this file by hand, which is error-prone and adds friction to the development process. To improve the local development experience, the `init.ps1` script should be enhanced to dynamically generate the `docker-compose.local.yml` file based on the project structure.
-   **Affected Files**:
    -   `init.ps1`
    -   `docker-compose.local.yml` (will become a generated file)
    -   `.gitignore`
-   **Action Points**:
    1.  **Enhance `init.ps1`**: Add logic to the PowerShell script to scan the `src` directory for service projects. A good convention would be to look for any directory containing a `Dockerfile`.
    2.  **Define Service Metadata**: The script could infer information from the project structure. For more complex settings, a small manifest file (e.g., `service.manifest.json`) could be placed in each service directory to define metadata like exposed ports or dependencies.
    3.  **Generate YAML**: Use PowerShell's capabilities to construct the `docker-compose.local.yml` file dynamically. The script would iterate through the discovered services and generate the corresponding service definitions (including build context, ports, and environment variables) in the YAML file.
    4.  **Update `.gitignore`**: Add `docker-compose.local.yml` to the `.gitignore` file, as it will now be a locally generated file.
    5.  **Create a Template (Optional)**: A `docker-compose.template.yml` could be created to hold the non-dynamic parts of the configuration, like the shared database service, which the script can use as a base.
-   **Verification**:
    -   Running `init.ps1` should generate a `docker-compose.local.yml` file that is functionally identical to the current manually maintained one.
    -   Running `docker-compose -f docker-compose.local.yml up` with the generated file should start all services correctly.
    -   Adding a new, simple service project with a Dockerfile and re-running `init.ps1` should automatically add the new service to the `docker-compose.local.yml` file.
