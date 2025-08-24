using System.Net;
using System.Net.Http;
using System.Text.Json;
using FluentAssertions;
using Insurance.Service.Clients;
using Insurance.Service.Contracts;
using Insurance.Service.Policies;
using Microsoft.Extensions.DependencyInjection;
using Insurance.Service.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Polly;
using ILogger = Serilog.ILogger;

namespace Insurance.UnitTests.Clients;

public class VehicleServiceClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly IVehicleServiceClient _client;

    public VehicleServiceClientTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var vehicleServiceClientConfiguration = new VehicleServiceClientConfiguration
        {
            MaxDegreeOfParallelismSingle = 5,
            MaxBatchSize = 2,
            MaxDegreeOfParallelismBatch = 1
        };

        var services = new ServiceCollection();
        services.AddSingleton(vehicleServiceClientConfiguration);
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<VehicleServiceClient>>(NullLogger<VehicleServiceClient>.Instance);

        services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>(c =>
            {
                c.BaseAddress = new Uri("http://localhost:5000");
            })
            .ConfigurePrimaryHttpMessageHandler(() => _mockHttpMessageHandler.Object);

        var serviceProvider = services.BuildServiceProvider();
        _client = serviceProvider.GetRequiredService<IVehicleServiceClient>();
    }

    [Fact]
    public async Task GetVehiclesBatchAsync_WhenApiReturnsNotFound_ShouldReturnEmptyList()
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
        var result = (await _client.GetVehiclesBatchAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVehiclesConcurrentlyAsync_WhenApiReturnsNotFound_ShouldReturnEmptyList()
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
        var result = (await _client.GetVehiclesConcurrentlyAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVehiclesBatchAsync_WhenApiReturnsSuccessWithVehicles_ShouldReturnMappedVehicles()
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
        var result = (await _client.GetVehiclesBatchAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var vehicle = result.First();
        vehicle.RegistrationNumber.Should().Be("ABC-123");
        vehicle.Make.Should().Be("Tesla");
        vehicle.Model.Should().Be("Model Y");
    }

    [Fact]
    public async Task GetVehiclesConcurrentlyAsync_WhenApiReturnsSuccessWithVehicles_ShouldReturnMappedVehicles()
    {
        // Arrange
        var registrationNumbers = new[] { "ABC-123" };
        var vehicleContract = new Vehicle.Service.Contracts.Vehicle { RegistrationNumber = "ABC-123", Make = "Tesla", Model = "Model Y" };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(vehicleContract), System.Text.Encoding.UTF8,
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
        var result = (await _client.GetVehiclesConcurrentlyAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var vehicle = result.First();
        vehicle.RegistrationNumber.Should().Be("ABC-123");
        vehicle.Make.Should().Be("Tesla");
        vehicle.Model.Should().Be("Model Y");
    }

    [Fact]
    public async Task GetVehiclesBatchAsync_WhenApiReturns500AndThenSuccess_ShouldRetryAndReturnMappedVehicles()
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

        var services = new ServiceCollection();
        services.AddSingleton(new VehicleServiceClientConfiguration { MaxDegreeOfParallelismSingle = 5, MaxBatchSize = 2, MaxDegreeOfParallelismBatch = 1 });
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<VehicleServiceClient>>(NullLogger<VehicleServiceClient>.Instance);
        services.AddSingleton(mockLogger.Object);

        services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>(c =>
            {
                c.BaseAddress = new Uri("http://localhost:5000");
            })
            .AddPolicyHandler(HttpClientPolicies.GetRetryPolicy(mockLogger.Object))
            .ConfigurePrimaryHttpMessageHandler(() => _mockHttpMessageHandler.Object);

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IVehicleServiceClient>();

        // Act
        var result = (await client.GetVehiclesBatchAsync(registrationNumbers)).ToArray();

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
    public async Task GetVehiclesConcurrentlyAsync_WhenApiReturns500AndThenSuccess_ShouldRetryAndReturnMappedVehicles()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var registrationNumbers = new[] { "ABC-123" };
        var vehicleContract = new Vehicle.Service.Contracts.Vehicle { RegistrationNumber = "ABC-123", Make = "Tesla", Model = "Model Y" };
        var successResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(vehicleContract), System.Text.Encoding.UTF8,
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

        var services = new ServiceCollection();
        services.AddSingleton(new VehicleServiceClientConfiguration { MaxDegreeOfParallelismSingle = 5, MaxBatchSize = 2, MaxDegreeOfParallelismBatch = 1 });
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<VehicleServiceClient>>(NullLogger<VehicleServiceClient>.Instance);
        services.AddSingleton(mockLogger.Object);

        services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>(c =>
            {
                c.BaseAddress = new Uri("http://localhost:5000");
            })
            .AddPolicyHandler(HttpClientPolicies.GetRetryPolicy(mockLogger.Object))
            .ConfigurePrimaryHttpMessageHandler(() => _mockHttpMessageHandler.Object);

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IVehicleServiceClient>();

        // Act
        var result = (await client.GetVehiclesConcurrentlyAsync(registrationNumbers)).ToArray();

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
    public async Task GetVehiclesBatchAsync_WhenCircuitIsBroken_ShouldReturnEmptyList()
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

        var services = new ServiceCollection();
        services.AddSingleton(new VehicleServiceClientConfiguration { MaxDegreeOfParallelismSingle = 5, MaxBatchSize = 2, MaxDegreeOfParallelismBatch = 1 });
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<VehicleServiceClient>>(NullLogger<VehicleServiceClient>.Instance);
        services.AddSingleton(mockLogger.Object);

        services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>(c =>
            {
                c.BaseAddress = new Uri("http://localhost:5000");
            })
            .AddPolicyHandler(HttpClientPolicies.GetFallbackPolicy(mockLogger.Object))
            .AddPolicyHandler(HttpClientPolicies.GetCircuitBreakerPolicy(mockLogger.Object, 1))
            .ConfigurePrimaryHttpMessageHandler(() => _mockHttpMessageHandler.Object);

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IVehicleServiceClient>();

        // Trip the circuit
        _ = await client.GetVehiclesBatchAsync(registrationNumbers);

        // Act
        var result = (await client.GetVehiclesBatchAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVehiclesConcurrentlyAsync_WhenCircuitIsBroken_ShouldReturnEmptyList()
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

        var services = new ServiceCollection();
        services.AddSingleton(new VehicleServiceClientConfiguration { MaxDegreeOfParallelismSingle = 5, MaxBatchSize = 2, MaxDegreeOfParallelismBatch = 1 });
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<VehicleServiceClient>>(NullLogger<VehicleServiceClient>.Instance);
        services.AddSingleton(mockLogger.Object);

        services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>(c =>
            {
                c.BaseAddress = new Uri("http://localhost:5000");
            })
            .AddPolicyHandler(HttpClientPolicies.GetSingleCallFallbackPolicy(mockLogger.Object))
            .AddPolicyHandler(HttpClientPolicies.GetCircuitBreakerPolicy(mockLogger.Object, 1))
            .ConfigurePrimaryHttpMessageHandler(() => _mockHttpMessageHandler.Object);

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IVehicleServiceClient>();

        // Trip the circuit
        _ = await client.GetVehiclesConcurrentlyAsync(registrationNumbers);

        // Act
        var result = (await client.GetVehiclesConcurrentlyAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVehiclesConcurrentlyAsync_WhenOneCallFails_ShouldReturnPartialData()
    {
        // Arrange
        var registrationNumbers = new[] { "REG1", "REG2", "REG3" };

        var vehicle1 = new Vehicle.Service.Contracts.Vehicle { RegistrationNumber = "REG1", Make = "Tesla", Model = "Model Y" };
        var vehicle3 = new Vehicle.Service.Contracts.Vehicle { RegistrationNumber = "REG3", Make = "Ford", Model = "Mustang" };

        var successResponse1 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(vehicle1), System.Text.Encoding.UTF8, "application/json")
        };
        var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var successResponse3 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(vehicle3), System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("REG1")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(successResponse1);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("REG2")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(errorResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("REG3")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(successResponse3);

        // Act
        var result = (await _client.GetVehiclesConcurrentlyAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(v => v.RegistrationNumber == "REG1");
        result.Should().Contain(v => v.RegistrationNumber == "REG3");
    }

    [Fact]
    public async Task GetVehiclesBatchAsync_WhenNumberOfVehiclesIsGreaterThanMaxBatchSize_ShouldChunkRequests()
    {
        // Arrange
        var registrationNumbers = new[] { "REG1", "REG2", "REG3" }; // Batch size is 2
        var vehicle1 = new Vehicle.Service.Contracts.Vehicle { RegistrationNumber = "REG1" };
        var vehicle2 = new Vehicle.Service.Contracts.Vehicle { RegistrationNumber = "REG2" };
        var vehicle3 = new Vehicle.Service.Contracts.Vehicle { RegistrationNumber = "REG3" };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Content != null && req.Content.ReadAsStringAsync().Result.Contains("REG1") && req.Content.ReadAsStringAsync().Result.Contains("REG2")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new[] { vehicle1, vehicle2 }), System.Text.Encoding.UTF8, "application/json")
            });

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Content != null && req.Content.ReadAsStringAsync().Result.Contains("REG3")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new[] { vehicle3 }), System.Text.Encoding.UTF8, "application/json")
            });

        // Act
        var result = (await _client.GetVehiclesBatchAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetVehiclesBatchAsync_WhenNumberOfVehiclesIsLessThanMaxBatchSize_ShouldSendOneRequest()
    {
        // Arrange
        var registrationNumbers = new[] { "REG1" }; // Batch size is 2
        var vehicleContracts = new[] { new Vehicle.Service.Contracts.Vehicle { RegistrationNumber = "REG1" } };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(vehicleContracts), System.Text.Encoding.UTF8, "application/json")
            });

        // Act
        var result = (await _client.GetVehiclesBatchAsync(registrationNumbers)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}