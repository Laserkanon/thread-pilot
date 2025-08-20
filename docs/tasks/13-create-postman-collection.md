### Task: Create and Share a Postman Collection

-   **Priority**: Low
-   **Complexity**: Low
-   **Description**: To improve the developer and tester experience, a Postman collection should be created for the service APIs. This provides a simple, interactive way to make requests to the services without needing to use `curl` or write client code. Since the services expose OpenAPI (Swagger) specifications, this collection can be easily generated.
-   **Affected Files**:
    -   A new file, e.g., `docs/api/postman_collection.json`

-   **Action Points**:

    1.  **Run the Services**: Start the services locally using `docker-compose up --build`. This will make the OpenAPI specification available via the Swagger UI.
    2.  **Get the OpenAPI Specification URL**: For each service, find the URL for the raw `swagger.json` file. This is usually linked from the top of the Swagger UI page (`/swagger/index.html`).
        -   **Vehicle.Service Spec URL**: `http://localhost:5081/swagger/v1/swagger.json`
        -   **Insurance.Service Spec URL**: `http://localhost:5082/swagger/v1/swagger.json`
    3.  **Import into Postman**:
        -   Open Postman.
        -   Click the "Import" button.
        -   Select the "Link" tab and paste the URL for the `Vehicle.Service`'s `swagger.json`.
        -   Follow the prompts to import the spec as a new collection.
        -   Repeat the process for the `Insurance.Service`.
    4.  **Organize the Collection**:
        -   Merge the two imported collections into a single, logical "ThreadPilot Services" collection if desired.
        -   Add example values for parameters (e.g., `199001011234` for `personalIdentityNumber`).
        -   Set up collection-level variables for the base URLs (e.g., `vehicle_service_url`, `insurance_service_url`) to make it easy to switch between local and other environments.
    5.  **Export the Collection**: Export the final, organized collection as a JSON file. Choose a suitable format like "Collection v2.1".
    6.  **Commit the Collection**: Save the exported JSON file in the repository, for example under `docs/api/threadpilot_postman_collection.json`.
    7.  **Update README**: Add a small section to the `README.md` file explaining that a Postman collection is available and how to import it.

-   **Verification**:
    -   Another team member should be able to import the committed JSON file into their Postman client.
    -   The imported collection should contain the correct requests for both services.
    -   Making a request from the imported collection to the locally running services should work successfully.
