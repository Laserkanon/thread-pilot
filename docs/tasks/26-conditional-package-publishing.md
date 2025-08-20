### Task: Conditionally Publish NuGet Packages Based on Contract Changes

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: To avoid publishing new package versions on every change to the `main` branch, the CI pipeline should only publish packages if there has been a change in one of the service contract projects. This task involves updating the CI workflow to detect changes in the `Insurance.Service.Contracts` and `Vehicle.Service.Contracts` directories and conditionally run the publish job.
-   **Affected Files**:
    -   `.github/workflows/ci.yml`

-   **Action Points**:

    1.  **Modify the `publish-packages` Job**: In `.github/workflows/ci.yml`, add a step at the beginning of the `publish-packages` job to check for changes in the contract directories. We will use the `dorny/paths-filter` GitHub Action for this.
    2.  **Add the Path Filter Step**: This step will check for changes in the specified paths and set an output variable (`contracts_changed`) to `true` if changes are detected.
        ```yaml
        - name: Check for changes in contracts
          id: changes
          uses: dorny/paths-filter@v2
          with:
            filters: |
              contracts:
                - 'src/Insurance/Insurance.Service.Contracts/**'
                - 'src/Vehicle/Vehicle.Service.Contracts/**'
        ```
    3.  **Update the Pack and Publish Steps**: Modify the `dotnet pack` and `dotnet nuget push` steps to only run if the `contracts_changed` output is `true`.
        ```yaml
        - name: Pack Vehicle.Contracts
          if: steps.changes.outputs.contracts == 'true'
          run: dotnet pack src/Vehicle/Vehicle.Service.Contracts/Vehicle.Service.Contracts.csproj -c Release -o ./artifacts

        - name: Pack Insurance.Contracts
          if: steps.changes.outputs.contracts == 'true'
          run: dotnet pack src/Insurance/Insurance.Service.Contracts/Insurance.Service.Contracts.csproj -c Release -o ./artifacts

        - name: Publish to GitHub Packages
          if: steps.changes.outputs.contracts == 'true'
          run: dotnet nuget push ./artifacts/*.nupkg --source "github" --skip-duplicate --api-key ${{ secrets.GITHUB_TOKEN }}
        ```
    4.  **Ensure the Job Runs**: The `publish-packages` job itself should still be configured to run on pushes to `main`, but the conditional steps inside it will prevent unnecessary package publishing.

-   **Verification**:
    -   Create a pull request with a change to a non-contract file (e.g., a README or a file in one of the service projects). After merging to `main`, the `publish-packages` job should run, but the pack and publish steps should be skipped.
    -   Create a pull request with a change to a file within `src/Insurance/Insurance.Service.Contracts/` or `src/Vehicle/Vehicle.Service.Contracts/`. After merging to `main`, the `publish-packages` job should run, and the pack and publish steps should execute, resulting in new package versions being published.
