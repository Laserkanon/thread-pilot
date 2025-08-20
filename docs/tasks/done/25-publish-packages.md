### Task: Publish NuGet Packages from CI Pipeline

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: To distribute shared libraries, the CI pipeline must build and publish NuGet packages to a registry. This task involves adding a new job or steps to the CI workflow to pack, version, and push `.nupkg` files for `Insurance.Contracts` and `Vehicle.Contracts` to GitHub Packages.
-   **Affected Files**:
    -   `.github/workflows/ci.yml`
    -   `nuget.config`

-   **Action Points**:

    1.  **Add a New CI Job for Publishing**: In `.github/workflows/ci.yml`, separate the build/test job from the publish job. Create a new job called `publish-packages` that `needs: build-test` to ensure it only runs if the tests pass.
    2.  **Condition the Job**: Configure the `publish-packages` job to run only on pushes to the `main` branch, or optionally when version tags are pushed.
        ```yaml
        publish-packages:
          runs-on: ubuntu-latest
          needs: build-test
          if: github.ref == 'refs/heads/main' || startsWith(github.ref, 'refs/tags/v')
          # ...
        ```
    3.  **Set Up .NET**: Add a step to install the correct .NET SDK for building and packing the projects.
        ```yaml
        - name: Set up .NET
          uses: actions/setup-dotnet@v4
          with:
            dotnet-version: '8.0.x'
        ```
    4.  **Pack the Projects**: Use `dotnet pack` to generate NuGet packages for the contracts libraries.
        ```yaml
        - name: Pack Vehicle.Contracts
          run: dotnet pack src/Vehicle/Vehicle.Contracts/Vehicle.Contracts.csproj -c Release -o ./artifacts

        - name: Pack Insurance.Contracts
          run: dotnet pack src/Insurance/Insurance.Contracts/Insurance.Contracts.csproj -c Release -o ./artifacts
        ```
    5.  **Publish Packages**: Add steps to push the generated `.nupkg` files to GitHub Packages. Use the `GITHUB_TOKEN` which is automatically available in the workflow.
        ```yaml
        - name: Publish to GitHub Packages
          run: dotnet nuget push ./artifacts/*.nupkg --source "github" --skip-duplicate --api-key ${{ secrets.GITHUB_TOKEN }}
        ```
        Configure `nuget.config` in the repository with the GitHub Packages source:
        ```xml
        <configuration>
          <packageSources>
            <add key="github" value="https://nuget.pkg.github.com/${{ github.repository_owner }}/index.json" />
          </packageSources>
        </configuration>
        ```
    6.  **Versioning Strategy**: Tag releases with semantic versions (`v1.2.3`). Use the tag value to version the packages dynamically.
        ```yaml
        run: dotnet pack src/Vehicle/Vehicle.Contracts/Vehicle.Contracts.csproj -c Release -o ./artifacts /p:Version=${GITHUB_REF#refs/tags/v}
        ```

-   **Verification**:
    -   After merging a PR into the `main` branch (or pushing a version tag), a new CI run should trigger.
    -   The `publish-packages` job should run and succeed.
    -   Navigate to the "Packages" section of the GitHub repository. The `vehicle-contracts` and `insurance-contracts` packages should be present with the correct version.
