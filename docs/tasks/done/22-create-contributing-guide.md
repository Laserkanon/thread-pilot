### Task: Create a `CONTRIBUTING.md` Guide

-   **Priority**: Medium
-   **Complexity**: Low
-   **Description**: To ensure a smooth and consistent workflow for all developers, a `CONTRIBUTING.md` file should be created. This file will serve as a guide for anyone who wants to contribute to the project, outlining the process for branching, submitting pull requests, and adhering to code standards.
-   **Affected Files**:
    -   New file: `CONTRIBUTING.md`

-   **Action Points**:

    1.  **Create `CONTRIBUTING.md`**: Create a new file named `CONTRIBUTING.md` in the root directory of the repository. GitHub automatically links to this file when a user creates a pull request or issue.
    2.  **Add a "How to Contribute" Section**: Start with a general section welcoming contributions.
    3.  **Define the Branching Strategy**: Document the branching model. A simple and common model is "GitHub Flow":
        -   The `main` branch is always deployable.
        -   To work on something new, create a descriptively named branch off of `main` (e.g., `feat/add-new-endpoint`, `fix/resolve-bug-123`).
        -   Commit to that branch locally and regularly push your work to the same named branch on the server.
    4.  **Define the Pull Request (PR) Process**: Outline the steps for submitting a pull request.
        -   When you are ready for feedback, open a pull request against the `main` branch.
        -   Ensure the PR has a clear title and a description explaining the "what" and "why" of the change.
        -   Ensure all CI checks (build, format, tests) are passing.
        -   At least one other developer must review and approve the PR before it can be merged.
    5.  **Reference Coding Standards**: Add a section on coding standards.
        -   Mention that the project uses the standard .NET coding conventions, enforced by the `.editorconfig` file.
        -   Remind developers to run `dotnet format` before committing to avoid CI failures.
    6.  **Reference Testing**: Remind developers to add or update tests for their changes. Any new feature should have corresponding unit or integration tests, and bug fixes should ideally include a regression test.

-   **Verification**:
    -   A new `CONTRIBUTING.md` file should exist in the root of the repository.
    -   The file should contain clear guidelines for the branching strategy and pull request process.
    -   When a developer opens a pull request, GitHub should display a banner with a link to the `CONTRIBUTING.md` file.
