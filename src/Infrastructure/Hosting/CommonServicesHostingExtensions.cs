using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Infrastructure.Hosting;

public static class CommonServicesHostingExtensions
{
    /// <summary>
    /// Controllers, Swagger, HealthChecks.
    /// </summary>
    public static WebApplicationBuilder AddCommonWebServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo { Title = builder.Environment.ApplicationName, Version = "v1" });

            o.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = "API Key needed to access the endpoints."
            });

            o.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey" 
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        builder.Services.AddHealthChecks();
        return builder;
    }

    /// <summary>
    /// OpenTelemetry Metrics with ASP.NET Core + HttpClient + Prometheus.
    /// </summary>
    public static WebApplicationBuilder AddOpenTelemetryWithPrometheus(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .WithMetrics(meterProviderBuilder =>
                meterProviderBuilder
                    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter());
        return builder;
    }
}