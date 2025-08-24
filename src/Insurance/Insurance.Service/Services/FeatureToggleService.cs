using Microsoft.Extensions.Options;

namespace Insurance.Service.Services;

public class FeatureToggleService : IFeatureToggleService
{
    private readonly IOptionsMonitor<FeatureToggleSettings> _optionsMonitor;

    public FeatureToggleService(IOptionsMonitor<FeatureToggleSettings> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public bool IsVehicleEnrichmentEnabled()
    {
        return _optionsMonitor.CurrentValue.EnableVehicleEnrichment;
    }

    public bool IsBatchVehicleCallEnabled()
    {
        return _optionsMonitor.CurrentValue.EnableBatchVehicleCall;
    }
}
