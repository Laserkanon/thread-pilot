using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class FeatureToggleService<T> : IFeatureToggleService<T> where T : class, new()
{
    private readonly IOptionsMonitor<T> _optionsMonitor;

    public FeatureToggleService(IOptionsMonitor<T> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public T Toggles => _optionsMonitor.CurrentValue;
}
