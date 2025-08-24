### Task: Update README with Vehicle Data Enrichment Strategy Configuration

-   **Priority**: Low
-   **Complexity**: Low
-   **Description**: The README.md file was missing a detailed explanation of the configuration parameters for the Vehicle Data Enrichment Strategy. This task adds a "Configuration" subsection to the "Vehicle Data Enrichment Strategy" section, detailing all the relevant parameters from `appsettings.json`. This improves clarity and makes it easier for developers to configure the system.
-   **Affected Files**:
    -   `README.md`
-   **Action Points**:
    1.  **Identify Parameters**: Investigated `appsettings.json` to identify all configuration parameters related to the vehicle data enrichment strategy.
    2.  **Update README**: Added a new "Configuration" subsection to the `README.md` file.
    3.  **Document Parameters**: Listed and described each parameter (`MaxDegreeOfParallelismSingle`, `MaxDegreeOfParallelismBatch`, `MaxBatchSize`, `EnableVehicleEnrichment`, `EnableBatchVehicleCall`).
    4.  **Refactor Documentation**: Removed redundant descriptions of the feature toggles from the main text to improve readability.
