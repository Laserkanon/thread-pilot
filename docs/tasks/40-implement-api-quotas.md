### Task: Implement API Quotas and Rate Limiting

-   **Priority**: Medium
-   **Complexity**: Medium
-   **Description**: To protect the services from being overwhelmed by traffic (either intentionally or unintentionally), a rate limiting and quota system should be implemented. This is a crucial resilience and security pattern, especially if the API is exposed to external clients or multiple tenants.
-   **Affected Files**:
    -   `src/Insurance/Insurance.Service/Program.cs`
    -   `src/Vehicle/Vehicle.Service/Program.cs`
    -   `appsettings.json` for both services.
-   **Action Points**:
    1.  **Add Rate Limiter Package**: Add the `Microsoft.AspNetCore.RateLimiting` NuGet package to both service projects.
    2.  **Configure Rate Limiting Policies**: In `Program.cs` for both services, configure different rate limiting policies. Examples could include:
        -   A **fixed window** limiter for anonymous requests (e.g., 100 requests per minute per IP address).
        -   A **sliding window** limiter for authenticated requests (e.g., 1000 requests per hour per API key).
        -   A **concurrency** limiter to limit the number of concurrent requests being processed.
    3.  **Add Rate Limiter Middleware**: Add the rate limiting middleware to the request pipeline using `app.UseRateLimiter()`.
    4.  **Apply Policies to Endpoints**: Apply the configured policies to specific endpoints or to all endpoints globally. This can be done using the `[EnableRateLimiting("PolicyName")]` attribute on controllers or by using `MapControllers().RequireRateLimiting("PolicyName")`.
    5.  **Externalize Configuration**: Move the rate limiting parameters (e.g., permit limit, window size) to `appsettings.json` to make them easily configurable per environment.
-   **Verification**:
    -   After implementing rate limiting, write an integration test or use a tool like `curl` in a loop to exceed the configured request limit.
    -   Verify that after the limit is reached, the API returns a `429 Too Many Requests` status code.
    -   Verify that the `Retry-After` header is present in the `429` response, indicating when the client can try again.
