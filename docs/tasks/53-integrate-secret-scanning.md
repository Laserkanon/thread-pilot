### Task: Integrate Secret Scanning into CI Pipeline

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: To prevent sensitive information like API keys, credentials, or private keys from being accidentally committed to the repository, an automated secret scanning tool should be integrated into the CI pipeline. This provides a critical layer of security by catching secrets before they become a persistent part of the git history.
-   **Affected Files**:
    -   `.github/workflows/ci.yml`
-   **Action Points**:
    1.  **Choose a Tool**: Select a secret scanning tool. [Gitleaks](https://github.com/gitleaks/gitleaks) is a popular open-source choice. GitHub also has its own secret scanning capabilities that can be enabled. This task will assume Gitleaks.
    2.  **Add Gitleaks Action to CI**: In `.github/workflows/ci.yml`, add a new job or a step at the beginning of an existing job to run the Gitleaks scan. There are pre-made GitHub Actions for this, such as `gitleaks/gitleaks-action`.
    3.  **Configure the Action**: The action should be configured to scan the entire history of the repository and fail the build if any secrets are found.
        ```yaml
        - name: Gitleaks Scan
          uses: gitleaks/gitleaks-action@v2
          env:
            GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
            GITLEAKS_LICENSE: ${{ secrets.GITLEAKS_LICENSE }} # If using the licensed version
        ```
    4.  **Create a Baseline (if needed)**: If the repository already contains secrets in its history, you may need to create a baseline file (`.gitleaks.toml`) to ignore them, and then work on removing them separately. For a clean repository, this should not be necessary.
-   **Verification**:
    -   The CI pipeline should include a Gitleaks scanning step.
    -   Intentionally add a file containing a fake secret (e.g., a string that looks like an AWS access key) and commit it to a feature branch.
    -   Push the branch and create a pull request. The CI pipeline should fail on the secret scanning step.
    -   After removing the secret and pushing again, the pipeline should pass.
