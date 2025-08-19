using FluentValidation;
using Vehicle.Service.Repositories;
using Vehicle.Service.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegistrationNumberValidator>();

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
public partial class Program { }
