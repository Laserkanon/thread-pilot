### Task: Publish Database Migrator Docker Images

-   **Priority**: Medium
-   **Complexity**: Medium
-   **Description**: The CI pipeline currently builds and publishes Docker images for the main services (`Insurance.Service`, `Vehicle.Service`), but it does not publish the images for the database migrator applications (`Insurance.Db`, `Vehicle.Db`). To enable running database migrations in containerized environments (e.g., as part of a CD pipeline using Kubernetes Jobs), these migrator images must also be published to the container registry.
-   **Affected Files**:
    -   `.github/workflows/ci.yml`
-   **Action Points**:
    1.  **Update `publish-images` Job**: In `.github/workflows/ci.yml`, modify the `publish-images` job.
    2.  **Update Path Filters**: Add path filters for the database projects (`Vehicle.Db/**` and `Insurance.Db/**`) to the `dorny/paths-filter` step. This will ensure the job only runs when relevant files change.
    3.  **Update Tagging Logic**: Extend the `generate-image-tags` step to generate tags for the new migrator images as well (e.g., `ghcr.io/.../vehicle-db-migrator:latest`).
    4.  **Add Build and Push Steps**: Add new `docker/build-push-action` steps for `vehicle-db-migrator` and `insurance-db-migrator`. These steps will use the Dockerfiles located in `src/Vehicle/Vehicle.Db/` and `src/Insurance/Insurance.Db/`.

-   **Verification**:
    -   After the changes are merged, make a change to one of the `Db` projects (e.g., `src/Vehicle/Vehicle.Db/Program.cs`).
    -   Verify that the CI pipeline triggers the `publish-images` job.
    -   Verify that the job successfully builds and pushes the corresponding database migrator image to the GitHub Container Registry.
    -   Check the GitHub Packages/Container Registry page for the repository to confirm the new image is present.
