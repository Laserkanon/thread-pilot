### Task: Add Code Coverage Reporting to CI

-   **Priority**: Medium
-   **Complexity**: Medium
-   **Description**: To ensure code quality and maintain testing standards, it's crucial to track code coverage. This task involves configuring the CI pipeline to generate a code coverage report during the test run and publish the results. This provides visibility into which parts of the codebase are tested and helps identify areas that need more test coverage.
-   **Affected Files**:
    -   `.github/workflows/ci.yml`
-   **Action Points**:
    1.  **Update Test Command**: In `.github/workflows/ci.yml`, modify the `Run Tests` step. The `dotnet test` command should be updated to include parameters for collecting code coverage. The `coverlet.collector` package is already referenced in the test projects, so we just need to enable it. The command should look like this: `dotnet test -c Release --no-build --collect:"XPlat Code Coverage"`.
    2.  **Specify Coverage Format**: To make the output usable by other tools (like report generators or services like Codecov), specify an output format. A common format is `cobertura`. The full command should be `dotnet test -c Release --no-build --collect:"XPlat Code Coverage" --results-directory ./coverage/`. The `--results-directory` argument is good practice to keep the output organized.
    3.  **Upload Coverage Report**: Add a new step to the CI workflow to upload the generated coverage report as a build artifact. This allows developers to download and inspect the report. Use the `actions/upload-artifact` action:
        ```yaml
        - name: Upload coverage report
          uses: actions/upload-artifact@v4
          with:
            name: code-coverage-report
            path: ./coverage/
        ```
    4.  **(Optional) Integrate with a Coverage Service**: For better visualization and history tracking, consider adding a step to upload the report to a service like [Codecov](https://about.codecov.io/) or [Coveralls](https://coveralls.io/). This typically involves using a dedicated GitHub Action for that service, e.g., `codecov/codecov-action`.
    5.  **Verification**: After modifying the CI workflow, push a commit and check the GitHub Actions run. Verify that the `Run Tests` step generates coverage files and the `Upload coverage report` step successfully archives them. Download the artifact to ensure it contains the coverage data.
