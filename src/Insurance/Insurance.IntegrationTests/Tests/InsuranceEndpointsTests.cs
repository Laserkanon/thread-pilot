using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Insurance.IntegrationTests.TestHelpers;
using Insurance.Service.Clients;
using Insurance.Service.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenTelemetry.Metrics;

namespace Insurance.IntegrationTests.Tests;

public class InsuranceEndpointsTests : IClassFixture<InsuranceTestWebApplicationFactory>
{
    private readonly InsuranceTestWebApplicationFactory _factory;

    public InsuranceEndpointsTests(InsuranceTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetInsurances_WhenPinIsValidAndHasInsurances_ReturnsOkWithData()
    {
        // Arrange
        var personalIdentityNumber = PersonalIdentityNumberGenerator.Generate();
        const string regNumber = "ABC123";
        
        using var scope = _factory.Services.CreateScope();
        var testDataSeeder = scope.ServiceProvider.GetRequiredService<ITestDataSeeder>();

        await testDataSeeder.InsertInsuranceAsync(personalIdentityNumber, ProductType.Car, 30, regNumber);
        await testDataSeeder.InsertInsuranceAsync(personalIdentityNumber, ProductType.Pet, 10);

        _factory.MockVehicleClient
            .Setup(c => c.GetVehiclesAsync(It.Is<string[]>(arr => arr.Contains(regNumber))))
            .ReturnsAsync(new List<Insurance.Service.Models.VehicleDetails>
            {
                new() { RegistrationNumber = regNumber, Make = "Volvo" }
            });

        var client = _factory.CreateClient().WithBearerToken(InsuranceTestWebApplicationFactory.TestKey, InsuranceTestWebApplicationFactory.TestIssuer, InsuranceTestWebApplicationFactory.TestAudience, new[] { "insurance:read" });

        // Act
        var response = await client.GetAsync($"/api/v1/insurances/{personalIdentityNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var insurances = await response.Content.ReadFromJsonAsync<List<Service.Contracts.Insurance>>();
        insurances.Should().NotBeNull();
        insurances.Should().HaveCount(2);

        var carInsurance = insurances.FirstOrDefault(i => i.Product == ProductType.Car);
        carInsurance.Should().NotBeNull();
        carInsurance.VehicleDetails.Should().NotBeNull();
        carInsurance.VehicleDetails!.Make.Should().Be("Volvo");
    }

    [Fact]
    public async Task GetInsurances_WhenPinIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        const string pin = "INVALID_PIN"; // expected to fail validation
        var client = _factory.CreateClient().WithBearerToken(InsuranceTestWebApplicationFactory.TestKey, InsuranceTestWebApplicationFactory.TestIssuer, InsuranceTestWebApplicationFactory.TestAudience, new[] { "insurance:read" });

        // Act
        var response = await client.GetAsync($"/api/v1/insurances/{pin}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetInsurances_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/insurances/some-pin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInsurances_WithTokenMissingScope_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient().WithBearerToken(InsuranceTestWebApplicationFactory.TestKey, InsuranceTestWebApplicationFactory.TestIssuer, InsuranceTestWebApplicationFactory.TestAudience, new[] { "some:other:scope" });

        // Act
        var response = await client.GetAsync("/api/v1/insurances/some-pin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
