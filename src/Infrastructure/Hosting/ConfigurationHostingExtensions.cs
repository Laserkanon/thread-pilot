using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Hosting
{
    /// <summary>
    /// Contains extension methods for IServiceCollection to simplify configuration registration.
    /// </summary>
    public static class ConfigurationHostingExtensions
    {
        /// <summary>
        /// Binds a configuration section to a specified type T and registers it as a singleton service.
        /// This makes the raw configuration object (T) available for dependency injection throughout the application.
        /// It also registers the configuration for the IOptions<T> pattern.
        /// </summary>
        /// <typeparam name="T">The type of the class to bind the configuration to. Must be a class.</typeparam>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="configuration">The application's IConfiguration instance.</param>
        /// <param name="configSection">The key of the configuration section to bind from (e.g., "MySettings").</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the configuration section is not found or cannot be bound.</exception>
        public static IServiceCollection AddConfiguration<T>(this IServiceCollection services, IConfiguration configuration, string configSection) where T : class
        {
            if (string.IsNullOrWhiteSpace(configSection))
            {
                throw new ArgumentNullException(nameof(configSection), "Configuration section name cannot be null or empty.");
            }

            var configObject = configuration.GetSection(configSection).Get<T>();

            if (configObject == null)
            {
                throw new InvalidOperationException($"Configuration section '{configSection}' could not be found or mapped to the type '{typeof(T).FullName}'.");
            }

            services.AddSingleton(configObject);
            services.Configure<T>(configuration.GetSection(configSection));

            return services;
        }
    }
}
