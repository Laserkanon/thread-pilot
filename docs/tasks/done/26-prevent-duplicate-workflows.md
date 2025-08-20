### Task: Prevent Duplicate GitHub Actions Workflow Runs

-   **Priority**: Medium
-   **Complexity**: Low
-   **Description**: The CI/CD pipeline was running twice for the same commit when a push was made to a branch with an open pull request. This was caused by the workflow being triggered by both the `push` and `pull_request` events. The workflow should only run once per commit.
-   **Affected Files**:
    -   `.github/workflows/ci.yml`
-   **Action Points**:
    1.  **Add Concurrency Control**: Add a `concurrency` block to the `.github/workflows/ci.yml` file.
    2.  **Configure Grouping**: Group concurrent jobs by workflow and branch (`github.head_ref || github.ref`).
    3.  **Enable Cancellation**: Set `cancel-in-progress: true` to ensure that any new workflow run cancels older, in-progress runs for the same group. This effectively prevents duplicate runs from completing for the same commit.
