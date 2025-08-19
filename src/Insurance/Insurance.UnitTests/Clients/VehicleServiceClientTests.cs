using System.Net;
using System.Text.Json;
using FluentAssertions;
using Insurance.Service.Clients;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace Insurance.UnitTests.Clients;

public class VehicleServiceClientTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly VehicleServiceClient _client;

    public VehicleServiceClientTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var mockConfSection = new Mock<IConfigurationSection>();
        mockConfSection.Setup(s => s.Value).Returns("http://localhost:5000");
        _mockConfiguration.Setup(c => c.GetSection("VehicleService:BaseUrl")).Returns(mockConfSection.Object);

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _client = new VehicleServiceClient(httpClient, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetVehiclesAsync_WhenApiReturnsNotFound_ShouldReturnEmptyList()
    {
        // Arrange
        var registrationNumbers = new[] { "NOT_FOUND" };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.GetVehiclesAsync(registrationNumbers);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVehiclesAsync_WhenApiReturnsSuccessWithVehicles_ShouldReturnMappedVehicles()
    {
        // Arrange
        var registrationNumbers = new[] { "ABC-123" };
        var vehicleContracts = new[]
        {
            new Vehicle.Service.Contracts.Vehicle { RegistrationNumber = "ABC-123", Make = "Tesla", Model = "Model Y" }
        };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(vehicleContracts), System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.GetVehiclesAsync(registrationNumbers);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var vehicle = result.First();
        vehicle.RegistrationNumber.Should().Be("ABC-123");
        vehicle.Make.Should().Be("Tesla");
        vehicle.Model.Should().Be("Model Y");
    }
}
