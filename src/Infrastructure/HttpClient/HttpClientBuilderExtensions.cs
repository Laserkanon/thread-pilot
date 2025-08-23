using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.HttpClient;

public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Configures an HttpClient with a base address and an API key from a specified configuration section.
    /// </summary>
    /// <param name="builder">The IHttpClientBuilder to configure.</param>
    /// <param name="targetHostName">The path to the configuration section in appsettings.json.</param>
    /// <returns>The IHttpClientBuilder for chaining.</returns>
    public static IHttpClientBuilder ConfigureHttpClientWithApiKey(
        this IHttpClientBuilder builder, 
        string targetHostName)
    {
        builder.ConfigureHttpClient((serviceProvider, client) =>
        {
            var clientConfigurationSection = GetClientConfigurationSectionPath(targetHostName);
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var serviceConfig = configuration.GetSection(clientConfigurationSection);
            
            if (!serviceConfig.Exists())
            {
                throw new InvalidOperationException($"Configuration section '{clientConfigurationSection}' not found.");
            }
            
            client.BaseAddress = new Uri(serviceConfig["BaseUrl"] ?? string.Empty);
            client.DefaultRequestHeaders.Add("x-api-key", serviceConfig["ApiKey"]);
        });

        return builder;
    }

    private static string GetClientConfigurationSectionPath(string serviceHost) => $"{serviceHost}.Client";
}