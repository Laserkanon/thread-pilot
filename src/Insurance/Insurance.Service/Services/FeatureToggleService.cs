namespace Insurance.Service.Services;

public class FeatureToggleService : IFeatureToggleService
{
    private readonly IConfiguration _configuration;

    public FeatureToggleService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsVehicleEnrichmentEnabled()
    {
        // Default to false if the setting is missing or invalid
        return _configuration.GetValue<bool>(FeatureToggles.EnableVehicleEnrichment);
    }
}
