### Task: Harden API Key Authentication Against Timing Attacks

-   **Priority**: High
-   **Complexity**: Low
-   **Description**: The current API key authentication mechanism uses a standard string comparison (`string.Equals`), which can be vulnerable to timing attacks. An attacker could potentially measure the time it takes for the comparison to fail to guess the API key character by character. This task is to replace the standard comparison with a constant-time comparison algorithm to mitigate this risk.
-   **Affected Files**:
    -   `src/Infrastructure/Hosting/ApiKeyAuthHandler.cs`
-   **Action Points**:
    1.  **Introduce a Constant-Time Comparison Method**: In `ApiKeyAuthHandler.cs`, create a private helper method that performs a constant-time comparison of two strings. This can be done by iterating through both byte arrays and performing bitwise operations in a way that doesn't short-circuit.
    2.  **Update Authentication Logic**: In `HandleAuthenticateAsync`, replace the `!apiKey.Equals(extractedApiKey.ToString())` call with a call to the new constant-time comparison method.
    3.  **Verification**: After implementation, run the services and verify that authentication still works with a valid API key and fails with an invalid one. Run unit tests if any exist for the `ApiKeyAuthHandler`.
