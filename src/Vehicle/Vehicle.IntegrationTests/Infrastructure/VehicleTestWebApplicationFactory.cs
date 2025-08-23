using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using Vehicle.IntegrationTests.TesHelpers;

namespace Vehicle.IntegrationTests.Infrastructure;

public class VehicleTestWebApplicationFactory : WebApplicationFactory<Program>
{
    // Expose the ApiKey so tests can use it
    public required string ApiKey { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Generate a unique ApiKey for each test run
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
                services.AddScoped<ITestDataSeeder, TestDataSeeder>();
                services.AddOpenTelemetry().WithMetrics(builder => builder.AddPrometheusExporter());
            });
    }
}