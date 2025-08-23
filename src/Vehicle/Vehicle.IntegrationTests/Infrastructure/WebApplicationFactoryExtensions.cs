namespace Vehicle.IntegrationTests.Infrastructure;

public static class WebApplicationFactoryExtensions
{
    /// <summary>
    /// Creates an HttpClient that is pre-configured with the correct API key header.
    /// </summary>
    public static HttpClient CreateAuthenticatedClient(this VehicleTestWebApplicationFactory factory)
    {
        // Create the default client
        var client = factory.CreateClient();

        // Add the API key to the default headers
        client.DefaultRequestHeaders.Add("x-api-key", factory.ApiKey);

        return client;
    }
}