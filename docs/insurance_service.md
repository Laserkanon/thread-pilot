# Insurance Service Architecture

```mermaid
graph TD
    subgraph "User"
        actor User
    end

    subgraph "Insurance.Service"
        IController[InsurancesController]
        IService[IInsuranceService]
        Service[InsuranceService]
        IRepo[IInsuranceRepository]
        Repo[InsuranceRepository]
        IClient[IVehicleServiceClient]
        Client[VehicleServiceClient]
        IFToggle[IFeatureToggleService]
        FToggle[FeatureToggleService]
        PinValidator[PersonalIdentifyNumberValidator]
        InsuranceModel[Insurance]
        VehicleModel[Vehicle]
        Db[(InsuranceDb)]
        AppSettings[appsettings.json]

        IController --> IService
        IController --> PinValidator
        IService -. implements .-> Service
        Service --> IRepo
        Service --> IClient
        Service --> IFToggle
        IRepo -. implements .-> Repo
        IClient -. implements .-> Client
        IFToggle -. implements .-> FToggle
        Repo --> Db
        FToggle --> AppSettings
        IController -. uses .-> InsuranceModel
        Service -. uses .-> InsuranceModel
        Repo -. uses .-> InsuranceModel
        Service -. uses .-> VehicleModel
        Client -. uses .-> VehicleModel
    end

    subgraph "Vehicle.Service"
        VehicleAPI[Vehicle Service API]
    end

    User --> IController
    Client --> VehicleAPI
```
