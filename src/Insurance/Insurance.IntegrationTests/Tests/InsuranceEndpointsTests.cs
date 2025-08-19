using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Insurance.IntegrationTests.TestHelpers;
using Insurance.Service.Clients;
using Insurance.Service.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Insurance.IntegrationTests.Tests;

public class InsuranceEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IVehicleServiceClient> _mockVehicleClient;

    public InsuranceEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _mockVehicleClient = new Mock<IVehicleServiceClient>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                //Add service that can setup test data
                services.AddScoped<ITestDataSeeder, TestDataSeeder>();
                
                //Mock external service
                services.AddScoped<IVehicleServiceClient>(_ => _mockVehicleClient.Object);
            });
        });
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

        _mockVehicleClient
            .Setup(c => c.GetVehiclesAsync(It.Is<string[]>(arr => arr.Contains(regNumber))))
            .ReturnsAsync(new List<Insurance.Service.Models.VehicleDetails>
            {
                new() { RegistrationNumber = regNumber, Make = "Volvo" }
            });

        var client = _factory.CreateClient();

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
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/insurances/{pin}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
