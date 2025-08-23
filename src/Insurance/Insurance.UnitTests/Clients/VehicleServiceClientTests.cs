using System.Net;
using System.Text.Json;
using FluentAssertions;
using Insurance.Service.Clients;
using Insurance.Service.Policies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Polly;
using ILogger = Serilog.ILogger;

namespace Insurance.UnitTests.Clients;

public class VehicleServiceClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly VehicleServiceClient _client;

    public VehicleServiceClientTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var mockConfSection = new Mock<IConfigurationSection>();
        mockConfSection.Setup(s => s.Value).Returns("http://localhost:5000");

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        _client = new VehicleServiceClient(httpClient, NullLogger<VehicleServiceClient>.Instance);
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
        var result = (await _client.GetVehiclesAsync(registrationNumbers)).ToArray();

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
            Content = new StringContent(JsonSerializer.Serialize(vehicleContracts), System.Text.Encoding.UTF8,
                "application/json")
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
        var result = (await _client.GetVehiclesAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var vehicle = result.First();
        vehicle.RegistrationNumber.Should().Be("ABC-123");
        vehicle.Make.Should().Be("Tesla");
        vehicle.Model.Should().Be("Model Y");
    }

    [Fact]
    public async Task GetVehiclesAsync_WhenApiReturns500AndThenSuccess_ShouldRetryAndReturnMappedVehicles()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var registrationNumbers = new[] { "ABC-123" };
        var vehicleContracts = new[]
        {
            new Vehicle.Service.Contracts.Vehicle { RegistrationNumber = "ABC-123", Make = "Tesla", Model = "Model Y" }
        };
        var successResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(vehicleContracts), System.Text.Encoding.UTF8,
                "application/json")
        };
        var errorResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(errorResponseMessage)
            .ReturnsAsync(successResponseMessage);

        var retryPolicy = HttpClientPolicies.GetRetryPolicy(mockLogger.Object);
        var httpClient = new HttpClient(new PollyHandler(retryPolicy)
        {
            InnerHandler = _mockHttpMessageHandler.Object
        })
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var client = new VehicleServiceClient(httpClient, NullLogger<VehicleServiceClient>.Instance);

        // Act
        var result = (await client.GetVehiclesAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetVehiclesAsync_WhenCircuitIsBroken_ShouldReturnEmptyList()
    {
        // Arrange
        var registrationNumbers = new[] { "ABC-123" };
        var errorResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var mockLogger = new Mock<ILogger>();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(errorResponseMessage);

        var circuitBreakerPolicy = HttpClientPolicies.GetCircuitBreakerPolicy(mockLogger.Object, 1);
        var fallbackPolicy = HttpClientPolicies.GetFallbackPolicy(mockLogger.Object);
        var policyWrap = Policy.WrapAsync(fallbackPolicy, circuitBreakerPolicy);

        var httpClient = new HttpClient(new PollyHandler(policyWrap)
        {
            InnerHandler = _mockHttpMessageHandler.Object
        })
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        var client = new VehicleServiceClient(httpClient, NullLogger<VehicleServiceClient>.Instance);

        // Trip the circuit
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetVehiclesAsync(registrationNumbers));

        // Act
        var result = (await client.GetVehiclesAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

public class PollyHandler : DelegatingHandler
{
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;

    public PollyHandler(IAsyncPolicy<HttpResponseMessage> policy)
    {
        _policy = policy;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return _policy.ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken);
    }
}