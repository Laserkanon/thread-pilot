using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Vehicle.Service.Repositories;
using Vehicle.Service.Services;
using Vehicle.Service.Validators;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

var jwt = builder.Configuration.GetSection("Jwt");
var authority = jwt["Authority"];
var audience  = jwt["Audience"];
var issuer    = jwt["Issuer"];
var devKey    = jwt["DevSymmetricKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        if (!string.IsNullOrWhiteSpace(authority))
        {
            // IdP mode
            options.Authority = authority;
            options.Audience  = audience;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        }
        else
        {
            // Local symmetric-key mode
            if (string.IsNullOrWhiteSpace(devKey))
                throw new InvalidOperationException("Jwt:DevSymmetricKey must be set when Jwt:Authority is empty.");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                ValidIssuer = issuer,
                ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(devKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        }
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("insurance:read",  p => p.RequireClaim("scope", "insurance:read"));
    options.AddPolicy("insurance:write", p => p.RequireClaim("scope", "insurance:write"));
    options.AddPolicy("vehicle:read",    p => p.RequireClaim("scope", "vehicle:read"));
    options.AddPolicy("vehicle:write",   p => p.RequireClaim("scope", "vehicle:write"));
    options.AddPolicy("AdminOnly",       p => p.RequireRole("admin"));
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IRegistrationNumberValidatorService, RegistrationNumberValidatorService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegistrationNumberValidator>();

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
///  <InternalsVisibleTo Include="Vehicle.IntegrationTests" />
/// </summary>
public  partial class Program { }
