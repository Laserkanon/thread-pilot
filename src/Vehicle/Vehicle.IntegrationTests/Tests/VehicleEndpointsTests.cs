using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using Vehicle.IntegrationTests.TesHelpers;

namespace Vehicle.IntegrationTests.Tests;

public class VehicleEndpointsTests : IClassFixture<VehicleTestWebApplicationFactory>
{
    private readonly VehicleTestWebApplicationFactory _factory;

    public VehicleEndpointsTests(VehicleTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetVehicle_WhenVehicleExists_ReturnsOk()
    {
        var registrationNumber = RegistrationNumberGenerator.NewReg();
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<ITestDataSeeder>();
        await seeder.InsertVehicleAsync(registrationNumber, "TestMake");

        var client = _factory.CreateClient().WithBearerToken(VehicleTestWebApplicationFactory.TestKey, VehicleTestWebApplicationFactory.TestIssuer, VehicleTestWebApplicationFactory.TestAudience, new[] { "vehicle:read" });

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
        var client = _factory.CreateClient().WithBearerToken(VehicleTestWebApplicationFactory.TestKey, VehicleTestWebApplicationFactory.TestIssuer, VehicleTestWebApplicationFactory.TestAudience, new[] { "vehicle:read" });

        // Act
        var response = await client.GetAsync($"/api/v1/vehicles/{registrationNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVehicle_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/vehicles/some-reg");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVehicle_WithTokenMissingScope_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient().WithBearerToken(VehicleTestWebApplicationFactory.TestKey, VehicleTestWebApplicationFactory.TestIssuer, VehicleTestWebApplicationFactory.TestAudience, new[] { "some:other:scope" });

        // Act
        var response = await client.GetAsync("/api/v1/vehicles/some-reg");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetVehiclesBatch_WithValidToken_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient().WithBearerToken(VehicleTestWebApplicationFactory.TestKey, VehicleTestWebApplicationFactory.TestIssuer, VehicleTestWebApplicationFactory.TestAudience, new[] { "vehicle:read" });

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/vehicles/batch", new[] { "reg1", "reg2" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
        content.Should().Contain("http_server_active_requests");
    }
}
