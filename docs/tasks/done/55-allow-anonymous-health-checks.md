### Task: Allow Anonymous Access to Health Check Endpoint

-   **Priority**: Medium
-   **Complexity**: Low
-   **Description**: The health check endpoints (`/healthz`) are currently protected by the default API key authentication scheme. This causes issues for automated systems like load balancers or container orchestrators, which need to poll the endpoint without credentials to determine application health. This task is to modify the application pipeline to allow anonymous access specifically to the health check endpoint.
-   **Affected Files**:
    -   `src/Infrastructure/Hosting/PipelineHostingExtensions.cs`
    -   `src/Infrastructure/Hosting/AllowAnonymousAttribute.cs` (new file)
-   **Action Points**:
    1.  **Modify Pipeline**: In `PipelineHostingExtensions.cs`, locate the `app.MapHealthChecks("/healthz")` call.
    2.  **Allow Anonymous Access**: Chain the `.WithMetadata(new AllowAnonymousAttribute())` method to the `MapHealthChecks` call to exempt it from the authentication and authorization pipeline.
    3.  **Add necessary `using` statement**: Add `using Microsoft.AspNetCore.Authorization;` to the top of `PipelineHostingExtensions.cs`.
    4.  **Create `AllowAnonymousAttribute.cs`**: Add a new file `src/Infrastructure/Hosting/AllowAnonymousAttribute.cs` that defines a custom `AllowAnonymousAttribute`.
    5.  **Verification**: Run the unit tests to ensure no existing functionality is broken. Manually confirm that the `/healthz` endpoint is accessible without an API key, while other endpoints still require it.
