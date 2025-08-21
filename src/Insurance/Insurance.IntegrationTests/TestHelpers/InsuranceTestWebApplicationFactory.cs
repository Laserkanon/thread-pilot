using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Insurance.Service.Clients;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenTelemetry.Metrics;

namespace Insurance.IntegrationTests.TestHelpers;

public class InsuranceTestWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestIssuer = "https://test-issuer";
    public const string TestAudience = "api://test-audience";
    public const string TestKey = "a-super-secret-key-that-is-long-enough-for-hs256";

    public readonly Mock<IVehicleServiceClient> MockVehicleClient = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Authority", "" }, // Ensure local mode
                { "Jwt:Issuer", TestIssuer },
                { "Jwt:Audience", TestAudience },
                { "Jwt:DevSymmetricKey", TestKey }
            });
        });

        builder.ConfigureServices(services =>
        {
            //Add service that can setup test data
            services.AddScoped<ITestDataSeeder, TestDataSeeder>();

            //Mock external service
            services.AddScoped<IVehicleServiceClient>(_ => MockVehicleClient.Object);

            services.AddOpenTelemetry().WithMetrics(builder => builder.AddPrometheusExporter());
        });
    }
}
