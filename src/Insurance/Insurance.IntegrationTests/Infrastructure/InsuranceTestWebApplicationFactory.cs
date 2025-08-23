using Insurance.IntegrationTests.TestHelpers;
using Insurance.Service.Clients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenTelemetry.Metrics;

namespace Insurance.IntegrationTests.Infrastructure;

public class InsuranceTestWebApplicationFactory : WebApplicationFactory<Program>
{
    // Expose the ApiKey so tests can use it
    public required string ApiKey { get; set; }

    public readonly Mock<IVehicleServiceClient> MockVehicleClient = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ApiKey = Guid.NewGuid().ToString();

        builder
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Authentication:ApiKey", ApiKey },
                });
            })
            .ConfigureServices(services =>
            {
                //Add service that can setup test data
                services.AddScoped<ITestDataSeeder, TestDataSeeder>();

                //Mock external service
                services.AddScoped<IVehicleServiceClient>(_ => MockVehicleClient.Object);

                services.AddOpenTelemetry().WithMetrics(builder => builder.AddPrometheusExporter());
            });
    }
}