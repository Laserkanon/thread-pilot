### Task: Implement API Key Authentication for Server-to-Server Communication

-   **Priority**: High
-   **Complexity**: low

Secure communication between services using an `x-api-key` header. Configuration is managed via `secrets.local.json` and a setup script (`init.ps1`) to avoid checking secrets into source control.

---

## Implementation Checklist

1.  **Define Secrets (`secrets.local.json.template`)**
   * Add `Authentication:ApiKey` for each service to define its own key.
   * Add a client-specific section (e.g., `Vehicle.Service.Client:ApiKey`) for inter-service calls.

2.  **Update Setup Script (`init.ps1`)**
   * Ensure the script recursively processes nested JSON to correctly set flattened keys (e.g., `Authentication:ApiKey`).

3.  **Implement Auth Handler (`ApiKeyAuthHandler.cs`)**
   * Create a central `AuthenticationHandler` that reads the `x-api-key` header, compares it to the `Authentication:ApiKey` value from configuration, and validates the request.

4.  **Register Services (`Program.cs`)**
   * In each service, register the authentication and authorization services, adding the `ApiKeyAuthHandler` as a scheme.
   * Add `app.UseAuthentication()` and `app.UseAuthorization()` to the middleware pipeline.

5.  **Configure `HttpClient`**
   * For inter-service communication (e.g., Insurance calling Vehicle), configure the `HttpClient` to automatically add the `x-api-key` header with the correct secret.

6.  **Protect Endpoints**
   * Add the `[Authorize]` attribute to controllers or specific endpoints that require protection.

---

## Verification

* **Check Secrets:** Run `init.ps1` and use `dotnet user-secrets list` to confirm keys are set correctly.
* **Direct API Calls:** Test endpoints with and without the `x-api-key` header to ensure a `401 Unauthorized` is returned for missing/invalid keys and `200 OK` for valid ones.
* **Inter-Service Calls:** Trigger a workflow that requires one service to call another to confirm the authenticated `HttpClient` works.

---

## Key Notes
* This pattern is for **trusted server-to-server** communication.
* **HTTPS is mandatory** in production.
* Use strong, unique keys (like GUIDs) for each environment.