### Task: Integrate Centralized Feature Toggle Management

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: For a more advanced setup, integrate a centralized feature toggle management service like LaunchDarkly or Azure App Configuration for real-time control over features in production.
-   **Action Points**:
    1.  **Research and Choose**: Investigate and decide on a centralized feature toggle service (e.g., LaunchDarkly, Azure App Configuration).
    2.  **Integrate SDK**: Add the required NuGet package/SDK for the chosen service to the `Infrastructure` project.
    3.  **Abstract Configuration**: Create a new configuration provider or update the existing `FeatureToggleService` to fetch toggles from the external service instead of `appsettings.json`.
    4.  **Update Setup**: Modify the `AddFeatureToggles` extension to allow for configuration of the external provider.
    5.  **Documentation**: Update `README.md` with instructions on how to configure and use the new centralized feature toggle system.
-   **Verification**:
    -   Run a service (e.g., `Insurance.Service`) locally.
    -   Verify that it correctly fetches feature toggles from the centralized service on startup.
    -   While the service is running, change a toggle value in the external service's UI.
    -   Verify that the application's behavior changes in real-time, reflecting the new value without a restart.
