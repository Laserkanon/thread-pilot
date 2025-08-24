using Insurance.Service.Contracts.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.HttpClient;

public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Configures an HttpClient with a base address and an API key from a specified configuration section.
    /// </summary>
    /// <param name="builder">The IHttpClientBuilder to configure.</param>
    /// <returns>The IHttpClientBuilder for chaining.</returns>
    public static IHttpClientBuilder ConfigureHttpClientWithApiKey<T>(
        this IHttpClientBuilder builder) where T : class, IClientConfiguration
    {
        builder.ConfigureHttpClient((serviceProvider, client) =>
        {
            var clientConfiguration = serviceProvider.GetRequiredService<T>();
            
            client.BaseAddress = new Uri(clientConfiguration.BaseUrl);
            client.DefaultRequestHeaders.Add("x-api-key", clientConfiguration.ApiKey);
        });

        return builder;
    }
}