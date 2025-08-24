namespace Infrastructure.FeatureToggle;

public interface IFeatureToggleService<T> where T : class, new()
{
    T Toggles { get; }
}
