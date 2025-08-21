using System.Text;
using FluentValidation;
using Insurance.Service.Clients;
using Insurance.Service.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Insurance.Service.Services;
using Insurance.Service.Validators;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Insurance.Service.Policies;
using Serilog;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());


// Add services to the container.
builder.Services.AddControllers();

// Domain services
builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddScoped<IFeatureToggleService, FeatureToggleService>();

// Http Client for Vehicle Service
builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>()
    .AddPolicyHandler((serviceProvider, _) =>
    {
        var logger = serviceProvider.GetRequiredService<ILogger>();
        return HttpClientPolicies.GetFallbackPolicy(logger);
    })
    .AddPolicyHandler(HttpClientPolicies.GetRetryPolicy())
    .AddPolicyHandler((serviceProvider, _) =>
    {
        var logger = serviceProvider.GetRequiredService<ILogger>();
        return HttpClientPolicies.GetCircuitBreakerPolicy(logger);
    });

// Add FluentValidation
builder.Services.AddScoped<IValidator<string>, PersonalIdentifyNumberValidator>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(meterProviderBuilder =>
        meterProviderBuilder
            .ConfigureResource(resource => resource
                .AddService(serviceName: builder.Environment.ApplicationName))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection if it's not explicitly disabled.
// This is useful for running in a container behind a TLS-terminating reverse proxy.
var disableHttpsRedirection = builder.Configuration.GetValue<bool>("DISABLE_HTTPS_REDIRECTION");
if (!disableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.MapPrometheusScrapingEndpoint();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.Run();

/// <summary>
/// InternalsVisibleTo doesnt work so adding this to bypass.
/// Alternatives:
/// - Spend more time investigating
/// - Change program.cs format
/// https://github.com/dotnet/AspNetCore.Docs/issues/23837
///  <InternalsVisibleTo Include="Insurance.IntegrationTests" />
/// </summary>
public partial class Program { }
