using FluentValidation;
using Insurance.Service.Clients;
using Insurance.Service.Repositories;
using Insurance.Service.Services;
using Insurance.Service.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Domain services
builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddScoped<IFeatureToggleService, FeatureToggleService>();

// Http Client for Vehicle Service
builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>();

// Add FluentValidation
builder.Services.AddScoped<IValidator<string>, PersonalIdentifyNumberValidator>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
