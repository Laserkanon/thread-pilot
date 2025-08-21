# Vehicle Service Architecture

```mermaid
graph TD;
    subgraph "Vehicle.Service";
        Controller[VehiclesController];
        IRepo[IVehicleRepository];
        Repo[VehicleRepository];
        IValidatorService[IRegistrationNumberValidatorService];
        ValidatorService[RegistrationNumberValidatorService];
        Model[Vehicle];
        Db[(VehicleDb)];
    end;

    Controller --> IRepo;
    Controller --> IValidatorService;
    IRepo <|--. Repo;
    IValidatorService <|--. ValidatorService;
    Repo --> Db;
    Controller ..> Model;
    Repo ..> Model;
```
