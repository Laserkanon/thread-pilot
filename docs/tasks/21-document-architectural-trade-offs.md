### Task: Document Architectural Trade-offs in README.md

-   **Priority**: Low
-   **Complexity**: Low
-   **Description**: A good `README.md` not only explains *what* the system does but also *why* it was designed a certain way. This task involves adding a new section to the main `README.md` to explicitly discuss the architectural trade-offs that were made. This provides valuable context for new developers and future architectural decisions.
-   **Affected Files**:
    -   `README.md`

-   **Action Points**:

    1.  **Create New Section**: Add a new section to `README.md` titled "Architectural Trade-offs". This could be a subsection under the existing "Architecture and Design Decisions".
    2.  **Add Content for Dapper vs. EF Core**: Write a paragraph explaining the choice of Dapper.
        -   **Chosen**: Dapper.
        -   **Reason**: High performance, lightweight, and full control over the generated SQL.
        -   **Trade-off**: Sacrificed the developer convenience of a full-fledged ORM like Entity Framework Core, which provides features like automatic change tracking and database migrations from code models. This choice prioritized performance over development speed for data access logic.
    3.  **Add Content for REST vs. Messaging**: Write a paragraph explaining the choice of synchronous REST calls for inter-service communication.
        -   **Chosen**: Synchronous REST/HTTP calls.
        -   **Reason**: Simplicity of implementation and immediate consistency for the enrichment workflow.
        -   **Trade-off**: This approach is less resilient than an asynchronous, event-driven architecture using a message broker (like RabbitMQ or Kafka). A messaging-based approach would offer better decoupling and fault tolerance but would introduce significant complexity in terms of infrastructure and handling eventual consistency.
    4.  **Add Content for Code-first vs. Database-first migrations**: Write a paragraph explaining the choice of DbUp for migrations.
        -   **Chosen**: DbUp (SQL script-based migrations).
        -   **Reason**: Gives database administrators (DBAs) full control and visibility over the exact SQL scripts being run against the database.
        -   **Trade-off**: This is more manual than a "code-first" migration approach (like in EF Core), where the database schema is generated from C# model classes. The chosen approach prioritizes database schema control over developer convenience.

-   **Verification**:
    -   The `README.md` file should be updated with the new "Architectural Trade-offs" section.
    -   The section should clearly and concisely explain the reasoning behind at least two or three major architectural decisions.
