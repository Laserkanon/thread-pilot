using FluentValidation;
using Infrastructure.Hosting;
using Infrastructure.HttpClient;
using Insurance.Service.Clients;
using Insurance.Service.Repositories;
using Insurance.Service.Services;
using Insurance.Service.Validators;
using Insurance.Service.Contracts;
using Insurance.Service.Policies;
using Insurance.Service.Settings;
using VehicleHost = Vehicle.Service.Contracts.Host;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args)
    .AddSerilogLogging()
    .AddCommonWebServices()
    .AddApiKeyAuthentication()
    .AddOpenTelemetryWithPrometheus();

// Domain services
builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddFeatureToggles<InsuranceFeatureToggles>(builder.Configuration);
builder.Services.AddScoped<IValidator<string>, PersonalIdentifyNumberValidator>();

var vehicleServiceSettings = new VehicleServiceClientConfiguration();
builder.Configuration.GetSection("Vehicle.Service.Client").Bind(vehicleServiceSettings);
builder.Services.AddSingleton(vehicleServiceSettings);

// Typed HttpClient for Vehicle Service
builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>()
    .ConfigureHttpClientWithApiKey<VehicleServiceClientConfiguration>()
    .AddPolicyHandler((sp, _) => HttpClientPolicies.GetFallbackPolicy(sp.GetRequiredService<ILogger>()))
    .AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp.GetRequiredService<ILogger>()))
    .AddPolicyHandler((sp, _) => HttpClientPolicies.GetCircuitBreakerPolicy(sp.GetRequiredService<ILogger>()));

var app = builder.Build();

app.UseSerilogDefaults();
app.UseCommonPipeline();

app.Run();

public partial class Program { }