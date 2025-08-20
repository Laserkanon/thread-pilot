### Task: Publish Docker Images from CI Pipeline

-   **Priority**: High
-   **Complexity**: Medium
-   **Description**: To enable deployments, the CI pipeline must build and publish the service Docker images to a container registry. This task involves adding a new job or steps to the CI workflow to build, tag, and push the Docker images for `Insurance.Service` and `Vehicle.Service` to the GitHub Container Registry (GHCR).
-   **Affected Files**:
    -   `.github/workflows/ci.yml`

-   **Action Points**:

    1.  **Add a New CI Job for Publishing**: In `.github/workflows/ci.yml`, it's good practice to separate the build/test job from the publish job. Create a new job called `publish-images` that `needs: build-test` to ensure it only runs if the tests pass.
    2.  **Condition the Job**: Configure the `publish-images` job to run only on pushes to the `main` branch, to avoid publishing images for every feature branch.
        ```yaml
        publish-images:
          runs-on: ubuntu-latest
          needs: build-test
          if: github.ref == 'refs/heads/main'
          # ...
        ```
    3.  **Log in to Registry**: Add a step to log in to the GitHub Container Registry (GHCR). Use the `docker/login-action` for this. The `GITHUB_TOKEN` is automatically available in the workflow.
        ```yaml
        - name: Log in to GitHub Container Registry
          uses: docker/login-action@v3
          with:
            registry: ghcr.io
            username: ${{ github.actor }}
            password: ${{ secrets.GITHUB_TOKEN }}
        ```
    4.  **Build and Push Images**: Add steps to build and push the Docker image for each service. The `docker/build-push-action` is excellent for this. It can handle building, tagging, and pushing in one step.
        -   **Tagging Strategy**: Tag images with both the Git commit SHA for traceability and `latest` for convenience.
        -   **Example for Vehicle.Service**:
            ```yaml
            - name: Build and push Vehicle.Service image
              uses: docker/build-push-action@v5
              with:
                context: .
                file: ./src/Vehicle/Vehicle.Service/Dockerfile
                push: true
                tags: |
                  ghcr.io/${{ github.repository }}/vehicle-service:latest
                  ghcr.io/${{ github.repository }}/vehicle-service:${{ github.sha }}
            ```
        -   Add a similar step for the `Insurance.Service`.
    5.  **Update Dockerfiles for Production**: The current `docker-compose.yml` uses the `dev` target in the Dockerfiles. The production images should be built from the final stage of the multi-stage Dockerfile. Ensure the Dockerfiles are structured correctly with a final `release` or `prod` stage that contains the compiled application, and that the `build-push-action` builds this final stage by default (or specify the `target`).

-   **Verification**:
    -   After merging a PR into the `main` branch, a new CI run should trigger.
    -   The `publish-images` job should run and succeed.
    -   Navigate to the "Packages" section of the GitHub repository. The `vehicle-service` and `insurance-service` images should be present, tagged with `latest` and the commit SHA.
