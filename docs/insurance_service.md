# Insurance Service Architecture

```mermaid
graph TD;
    subgraph "User";
        actor User;
    end;

    subgraph "Insurance.Service";
        IController[InsurancesController];
        IService[IInsuranceService];
        Service[InsuranceService];
        IRepo[IInsuranceRepository];
        Repo[InsuranceRepository];
        IClient[IVehicleServiceClient];
        Client[VehicleServiceClient];
        IFToggle[IFeatureToggleService];
        FToggle[FeatureToggleService];
        PinValidator[PersonalIdentifyNumberValidator];
        InsuranceModel[Insurance];
        VehicleModel[Vehicle];
        Db[(InsuranceDb)];
        AppSettings[appsettings.json];

        IController --> IService;
        IController --> PinValidator;
        IService <|--. Service;
        Service --> IRepo;
        Service --> IClient;
        Service --> IFToggle;
        IRepo <|--. Repo;
        IClient <|--. Client;
        IFToggle <|--. FToggle;
        Repo --> Db;
        FToggle --> AppSettings;
        IController ..> InsuranceModel;
        Service ..> InsuranceModel;
        Repo ..> InsuranceModel;
        Service ..> VehicleModel;
        Client ..> VehicleModel;
    end;

    subgraph "Vehicle.Service";
        VehicleAPI[Vehicle Service API];
    end;

    User --> IController;
    Client --> VehicleAPI;
```
