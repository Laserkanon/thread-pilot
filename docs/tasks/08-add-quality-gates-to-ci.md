### Task: Add Code Quality Gates to CI Pipeline

-   **Priority**: Medium
-   **Complexity**: Medium
-   **Description**: To maintain a high standard of code quality and consistency across the codebase, automated quality gates should be added to the CI pipeline. This task involves adding steps for code formatting checks and static analysis to ensure that all committed code adheres to the project's standards.
-   **Affected Files**:
    -   `.github/workflows/ci.yml`
    -   A new `.editorconfig` file at the root of the repository.

-   **Action Points**:

    1.  **Create `.editorconfig` File**: Create a central `.editorconfig` file at the root of the repository. This file will define the coding styles, formatting rules, and analyzer severities for the entire solution. You can start with the default .NET configuration and customize it as needed.
        ```editorconfig
        # .NET coding conventions
        [*.cs]
        dotnet_diagnostic.IDE0055.severity = error

        # C# formatting rules
        [*.cs]
        csharp_space_between_parentheses = false
        ```
    2.  **Add Formatting Check to CI**: In `.github/workflows/ci.yml`, add a new step *before* the `Build` step to check for formatting issues. The `dotnet format` command with the `--verify-no-changes` flag will fail if the code is not formatted correctly.
        ```yaml
        - name: Check formatting
          run: dotnet format --verify-no-changes --verbosity detailed
        ```
    3.  **Enhance Static Analysis**: The projects already use `<WarningsAsErrors />`, which is good. The `.editorconfig` file will centralize the configuration of which warnings should be treated as errors. The existing `Build` step in the CI will automatically pick up these rules and fail if any are violated. No changes are needed to the build step itself, but the `.editorconfig` file is the key enabler.
    4.  **(Optional) Add SonarCloud Analysis**: For more advanced static analysis, vulnerability scanning, and code smell detection, integrate SonarCloud.
        -   Set up a SonarCloud project linked to the GitHub repository.
        -   Add the `SONAR_TOKEN` to GitHub secrets.
        -   Add the SonarCloud scan steps to the CI workflow, which typically wrap the build and test commands to collect analysis data.
    5.  **Document the Process**: Add a small section to the `README.md` or a new `CONTRIBUTING.md` explaining that developers should run `dotnet format` locally before committing their changes.

-   **Verification**:
    -   After adding the `Check formatting` step, intentionally commit a poorly formatted C# file. The CI pipeline should fail on the new step.
    -   After fixing the formatting and pushing again, the CI pipeline should pass the formatting check.
    -   Violating a rule configured as an `error` in `.editorconfig` should cause the `Build` step to fail.
