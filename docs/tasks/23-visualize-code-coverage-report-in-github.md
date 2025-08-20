### Task: Integrate Code Coverage Service with CI

-   **Priority**: Medium
-   **Complexity**: Medium
-   **Description**: To improve visibility of code quality, the code coverage report should be easily accessible. This task involves integrating a third-party code coverage service (like Codecov or Coveralls) with the CI pipeline. This will allow developers to see coverage reports directly in pull requests, making it easier to assess the impact of their changes on test coverage.
-   **Affected Files**:
    -   `.github/workflows/ci.yml`
-   **Action Points**:
    1.  **Choose a Coverage Service**: Decide between services like Codecov or Coveralls. For this task, we will use Codecov as it's a popular choice.
    2.  **Add Service Integration to CI**: In `.github/workflows/ci.yml`, add a new step after the tests are run and the coverage report is generated. This step will use the official GitHub Action for the chosen service to upload the report. For Codecov, this would be the `codecov/codecov-action`.
    3.  **Configure the Action**: The action will need to be configured. This may involve specifying the path to the coverage file. The `XPlat Code Coverage` collector generates reports in the `cobertura` format, which is supported by most services. The file is located in the `./coverage/` directory.
    4.  **Handle Secrets**: The action might require a secret token to upload reports to the service. This token needs to be added to the GitHub repository's secrets and then used in the workflow file. The task will assume a secret named `CODECOV_TOKEN` is available.
    5.  **Verification**: After modifying the CI workflow, push a commit and check the GitHub Actions run. Verify that the new step successfully uploads the coverage report and that the report is visible on the coverage service's dashboard and ideally in the pull request.
