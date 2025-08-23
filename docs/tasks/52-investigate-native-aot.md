### Task: Investigate Native AOT Compilation

-   **Priority**: Low
-   **Complexity**: High
-   **Description**: To significantly reduce memory footprint and startup times, making the services more efficient and scalable (especially in serverless or containerized environments), this task is to investigate compiling the services to Native AOT (Ahead-of-Time). This is an exploratory task, as not all .NET features and libraries are fully compatible with Native AOT.
-   **Affected Files**:
    -   `.csproj` files for both services.
    -   `Program.cs` files for both services.
-   **Action Points**:
    1.  **Analyze Compatibility**: Review the project's NuGet dependencies for known Native AOT compatibility issues. Libraries that use reflection heavily or run-time code generation may not be compatible. The `Microsoft.CodeAnalysis.PublicApiAnalyzers` package can help identify potential issues.
    2.  **Enable AOT Publishing**: In a separate branch, modify a service's `.csproj` file to enable AOT compilation by adding `<PublishAot>true</PublishAot>`.
    3.  **Refactor for Trim-Safety**: The project must be made "trim-safe" and "source-generator-safe". This involves refactoring any code that relies on dynamic reflection to use source generators instead (e.g., for JSON serialization, use the `JsonSerializerContext`).
    4.  **Attempt to Build**: Try to publish the application using `dotnet publish -r <runtime-identifier>`. Address any build warnings or errors that arise. This will likely be an iterative process.
    5.  **Measure Performance**: If the build is successful, measure the performance of the Native AOT-compiled application. Key metrics to compare against the JIT version are:
        -   Container image size.
        -   Application startup time.
        -   Memory usage under load.
-   **Verification**:
    -   The primary outcome is a report or summary of the findings. Can the services be compiled with Native AOT? What were the challenges? What are the performance benefits?
    -   If successful, a separate branch containing a working AOT-compiled version of a service should be available for review.
