namespace Infrastructure.Services;

public interface IFeatureToggleService<T> where T : class, new()
{
    T Toggles { get; }
}
