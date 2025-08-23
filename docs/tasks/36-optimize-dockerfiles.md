### Task: Optimize Dockerfiles for Security and Performance

-   **Priority**: Low
-   **Complexity**: Medium
-   **Description**: The Dockerfiles for the services can be optimized to improve build performance (via layer caching) and enhance security by running the application as a non-root user.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Dockerfile`
    -   `src/Vehicle/Vehicle.Service/Dockerfile`
-   **Action Points**:
    1.  **Optimize Layer Caching**: Modify the Dockerfiles to copy the `.sln` and `.csproj` files first, then run `dotnet restore`. After the restore step, copy the rest of the source code. This ensures that the dependency layer is only rebuilt when project files change, not on every code change.
    2.  **Implement Non-Root User**:
        -   In the `prod` stage of each Dockerfile, add commands to create a new, non-privileged user and user group (e.g., `appuser`).
        -   Set the `USER` instruction to switch to this new user before the `ENTRYPOINT`.
        -   Ensure the application files in the `/app` directory are owned by the new user.
    3.  **Review `dev` Stage**: Evaluate if the `dev` stage in the Dockerfiles is still necessary. If its only purpose is to install `curl` and set the environment, consider removing it and managing local development configuration entirely within `docker-compose.local.yml`. This simplifies the Dockerfile and slims down the resulting images.
    4.  **Verification**: After updating the Dockerfiles, run `docker-compose -f docker-compose.local.yml up --build` to ensure the images build successfully and the services run correctly. Verify that the application process inside the container is running under the new `appuser` account.
