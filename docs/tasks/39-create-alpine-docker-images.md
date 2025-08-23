### Task: Create Alpine-based Docker Images for Production

-   **Priority**: Low
-   **Complexity**: Medium
-   **Description**: The current production Docker image is based on the default Debian-based ASP.NET runtime image. To create smaller and more secure production images, a new build stage should be added to the Dockerfiles that targets the Alpine Linux distribution, which uses the `musl` C standard library instead of `glibc`.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Dockerfile`
    -   `src/Vehicle/Vehicle.Service/Dockerfile`
-   **Action Points**:
    1.  **Add Alpine Build Stage**: In each service's Dockerfile, add a new stage for the Alpine build. This will require using an Alpine-specific .NET SDK image (e.g., `mcr.microsoft.com/dotnet/sdk:8.0-alpine`).
    2.  **Publish for Alpine**: In the new stage, the `dotnet publish` command must be modified to target the Alpine runtime identifier (`linux-musl-x64`). This ensures the correct native dependencies are included.
        ```dockerfile
        RUN dotnet publish -c ${BUILD_CONFIGURATION} -o /app/publish -r linux-musl-x64 --self-contained false
        ```
    3.  **Create Alpine Final Stage**: Create a new final stage based on the Alpine ASP.NET runtime image (`mcr.microsoft.com/dotnet/aspnet:8.0-alpine`). Copy the published output from the Alpine build stage into this final stage.
    4.  **Install `icu-libs`**: The Alpine .NET runtime requires globalization libraries to be installed manually. Add `RUN apk add --no-cache icu-libs` to the final Alpine stage.
    5.  **Update `docker-compose` (Optional)**: If desired, a new `docker-compose.prod.yml` could be created to build and run the new Alpine-based images.
-   **Verification**:
    -   The services should build successfully using the new Dockerfile stages.
    -   The resulting Docker images should be significantly smaller than the Debian-based ones.
    -   The services should start and run correctly inside the Alpine-based containers. All API endpoints should function as expected.
