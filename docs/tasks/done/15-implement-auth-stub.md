### Task: Implement a Stub for JWT-Based Authentication

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: The APIs are currently unsecured, allowing anonymous access. This task involves adding the foundational pieces for JWT (JSON Web Token) bearer authentication. This will be a "stub" implementation: the middleware and attributes will be put in place, but the token validation will be simplified for local development, preparing the system for a full integration with an identity provider in the future.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Program.cs`
    -   `src/Vehicle/Vehicle.Service/Program.cs`
    -   `src/Insurance/Insurance.Service/Controllers/InsurancesController.cs`
    -   `src/Vehicle/Vehicle.Service/Controllers/VehiclesController.cs`
    -   `src/Insurance/Insurance.Service/Insurance.Service.csproj`
    -   `src/Vehicle/Vehicle.Service/Vehicle.Service.csproj`

-   **Action Points**:

    1.  **Add NuGet Package**: In both the `Insurance.Service.csproj` and `Vehicle.Service.csproj` files, add a package reference to `Microsoft.AspNetCore.Authentication.JwtBearer`.
    2.  **Configure Authentication Services**: In the `Program.cs` of both services, add the authentication services to the DI container.
        ```csharp
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                // For a real app, these would come from config & point to an identity provider
                options.Authority = "https://localhost:5001"; // Placeholder
                options.Audience = "weatherapi"; // Placeholder

                // For local development, we can disable HTTPS metadata requirement
                if (builder.Environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }
            });
        ```
    3.  **Configure Authorization Services**: In `Program.cs`, add the authorization services. A default policy requiring an authenticated user is a good start.
        ```csharp
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                // In a real app, you'd also require specific scopes/claims
                // policy.RequireClaim("scope", "weather:read");
            });
        });
        ```
    4.  **Enable Middleware**: In `Program.cs` for both services, enable the authentication and authorization middleware. This must be placed *after* `app.UseRouting()` and *before* `app.UseEndpoints()` (or `app.MapControllers()` in this case).
        ```csharp
        app.UseAuthentication();
        app.UseAuthorization();
        ```
    5.  **Secure Endpoints**: In the controllers (`InsurancesController.cs`, `VehiclesController.cs`), add the `[Authorize]` attribute to the controller class or individual action methods to protect them.
        ```csharp
        [ApiController]
        [Route("api/v1/insurances")]
        [Authorize] // Add this attribute
        public class InsurancesController : ControllerBase
        // ...
        ```
    6.  **Update `docker-compose.yml` (Optional but Recommended)**: To make local testing possible, you could add a mock identity server (like a simple Duende IdentityServer project) to the docker-compose setup to issue tokens. For a true "stub", this can be skipped, and testing can be done by manually generating a token and assuming it will work once a real identity provider is configured.

-   **Verification**:
    -   After applying the changes, running the services, and making an API request *without* an `Authorization` header, the API should return a `401 Unauthorized` status code.
    -   Making a request with a valid (even if self-generated for testing) JWT in the `Authorization: Bearer <token>` header should result in a `200 OK` (assuming the request is otherwise valid).
