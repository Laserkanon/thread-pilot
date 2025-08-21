---
title: Update and Add Architecture Diagrams
---

## Description

The existing architecture diagrams were outdated and used a format (PlantUML) that was not consistent with the rest of the documentation. This task was to update the architecture diagrams to reflect the current state of the codebase, convert them to Mermaid syntax, and add new sequence diagrams to provide more detailed insights into the system's behavior.

## Benefits of this Change

### 1. Up-to-Date and Accurate Documentation

The architecture diagrams for the `Insurance.Service` and `Vehicle.Service` were updated to include all current components, such as validators and feature toggle services. This ensures that developers have an accurate, high-level view of the system architecture.

### 2. Standardized and Maintainable Format

All diagrams were converted from PlantUML to Mermaid syntax. Mermaid is a widely-used, markdown-inspired syntax that is easy to read and maintain. Being text-based, the diagrams can be version-controlled alongside the source code, which helps prevent documentation rot.

### 3. Deeper Insight into System Behavior

Three new sequence diagrams were added to illustrate the runtime execution flow of the main controller methods:
- `InsurancesController.GetInsurances`
- `VehiclesController.GetVehicle`
- `VehiclesController.GetVehiclesBatch`

These diagrams provide a clear, step-by-step view of how requests are processed through the different layers of the services, making it easier for developers to understand the system's runtime behavior and debug issues.

## Acceptance Criteria

- All architecture diagrams in the `/docs` directory are up-to-date and in Mermaid format.
- The old PlantUML (`.puml`) files are removed.
- New sequence diagrams for the three main controller methods are added to a new `docs/sequence-diagrams` directory.
- All diagrams are validated to ensure they render correctly.
