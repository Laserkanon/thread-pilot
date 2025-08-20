using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using Vehicle.IntegrationTests.TesHelpers;

namespace Vehicle.IntegrationTests.Tests;

public class VehicleEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public VehicleEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ITestDataSeeder, TestDataSeeder>();
                services.AddOpenTelemetry().WithMetrics(builder => builder.AddPrometheusExporter());
            });
        });
    }

    [Fact]
    public async Task GetVehicle_WhenVehicleExists_ReturnsOk()
    {
        var registrationNumber = RegistrationNumberGenerator.NewReg();
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<ITestDataSeeder>();
        await seeder.InsertVehicleAsync(registrationNumber, "TestMake");

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/vehicles/{registrationNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var vehicle = await response.Content.ReadFromJsonAsync<Service.Contracts.Vehicle>();
        vehicle.Should().NotBeNull();
        vehicle!.RegistrationNumber.Should().Be(registrationNumber);
        vehicle.Make.Should().Be("TestMake");
    }

    [Fact]
    public async Task GetVehicle_WhenVehicleDoesNotExist_ReturnsNotFound()
    {
        // Arrange: ensure you don't insert this reg number
        const string registrationNumber = "MISSING";
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/vehicles/{registrationNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMetrics_ReturnsOkAndPrometheusContent()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("# TYPE http_server_request_duration_seconds histogram");
    }
}
