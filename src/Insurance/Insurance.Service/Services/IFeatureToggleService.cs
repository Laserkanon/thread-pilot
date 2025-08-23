namespace Insurance.Service.Services;

public interface IFeatureToggleService
{
    bool IsVehicleEnrichmentEnabled();
    bool IsBatchVehicleCallEnabled();
}
