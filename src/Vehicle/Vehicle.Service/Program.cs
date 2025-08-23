using FluentValidation;
using Infrastructure.Hosting;
using Vehicle.Service.Repositories;
using Vehicle.Service.Services;
using Vehicle.Service.Validators;

var builder = WebApplication.CreateBuilder(args)
    .AddSerilogLogging()
    .AddCommonWebServices()
    .AddApiKeyAuthentication()
    .AddOpenTelemetryWithPrometheus();

// App-specific DI
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IRegistrationNumberValidatorService, RegistrationNumberValidatorService>();
builder.Services.AddValidatorsFromAssemblyContaining<RegistrationNumberValidator>();

var app = builder.Build();

app.UseSerilogDefaults();
app.UseCommonPipeline();

app.Run();

public partial class Program { }