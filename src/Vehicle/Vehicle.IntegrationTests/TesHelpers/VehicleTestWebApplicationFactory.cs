using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Vehicle.IntegrationTests.TesHelpers;

public class VehicleTestWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestIssuer = "https://test-issuer";
    public const string TestAudience = "api://test-audience";
    public const string TestKey = "a-super-secret-key-that-is-long-enough-for-hs256";

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
            services.AddScoped<ITestDataSeeder, TestDataSeeder>();
            services.AddOpenTelemetry().WithMetrics(builder => builder.AddPrometheusExporter());
        });
    }
}
