using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Hosting;

public static class FeatureToggleHostingExtensions
{
    public static IServiceCollection AddFeatureToggles<T>(this IServiceCollection services, IConfiguration configuration) where T : class, new()
    {
        services.Configure<T>(configuration.GetSection("FeatureToggles"));
        services.AddScoped<IFeatureToggleService<T>, FeatureToggleService<T>>();
        return services;
    }
}
