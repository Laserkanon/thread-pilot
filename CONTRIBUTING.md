# How to Contribute

We welcome contributions from the community! Whether you're fixing a bug, adding a new feature, or improving documentation, your help is appreciated. Please take a moment to review this document to ensure a smooth and collaborative process.

## Branching Strategy

We follow the GitHub Flow model, which is a simple and effective branching strategy:

-   The `main` branch is always considered stable and deployable.
-   To start working on a new feature or bug fix, create a new branch from `main`. Name your branch descriptively, using a prefix like `feat/` for features or `fix/` for bug fixes (e.g., `feat/add-new-endpoint`, `fix/resolve-bug-123`).
-   Commit your changes to your branch locally. Make sure to push your branch to the remote repository regularly to back up your work.

## Pull Request (PR) Process

Once your changes are ready for review, follow these steps to open a pull request:

1.  **Open a Pull Request**: Push your branch to the remote repository and open a pull request against the `main` branch.
2.  **Provide a Clear Description**: The pull request should have a clear and concise title. The description should explain the "what" and "why" of your changes, providing context for the reviewer.
3.  **Ensure CI Checks Pass**: Before a pull request can be merged, all automated checks (build and tests) must pass. Make sure to run these checks locally before pushing.
4.  **Get a Review**: At least one other developer must review and approve your pull request. Address any feedback or requested changes by pushing new commits to your branch.

## Testing

We believe in the importance of testing. When you contribute code, please ensure that you add or update tests as needed:

-   **New Features**: Any new feature should be accompanied by corresponding unit or integration tests to verify its correctness.
-   **Bug Fixes**: If you are fixing a bug, it is highly recommended to add a regression test. This helps ensure the bug does not reappear in the future.

Thank you for contributing to our project!
