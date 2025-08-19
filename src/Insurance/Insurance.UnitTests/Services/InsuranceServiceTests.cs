using FluentAssertions;
using Insurance.Service.Clients;
using Insurance.Service.Repositories;
using Insurance.Service.Services;
using Moq;
using VehicleDetails = Insurance.Service.Models.VehicleDetails;

namespace Insurance.UnitTests.Services;

public class InsuranceServiceTests
{
    private readonly Mock<IInsuranceRepository> _mockInsuranceRepository;
    private readonly Mock<IVehicleServiceClient> _mockVehicleServiceClient;
    private readonly Mock<IFeatureToggleService> _mockFeatureToggleService;
    private readonly InsuranceService _insuranceService;

    public InsuranceServiceTests()
    {
        _mockInsuranceRepository = new Mock<IInsuranceRepository>();
        _mockVehicleServiceClient = new Mock<IVehicleServiceClient>();
        _mockFeatureToggleService = new Mock<IFeatureToggleService>();

        // Always enable the feature toggle for these tests unless specified otherwise
        _mockFeatureToggleService.Setup(f => f.IsVehicleEnrichmentEnabled()).Returns(true);

        _insuranceService = new InsuranceService(
            _mockInsuranceRepository.Object,
            _mockVehicleServiceClient.Object,
            _mockFeatureToggleService.Object);
    }

    [Fact]
    public async Task GetInsurancesForPinAsync_ShouldReturnCorrectlyMappedContracts()
    {
        // Arrange
        const string pin = "199001011234";
        var insuranceEntities = new Insurance.Service.Models.Insurance[]
        {
            new() { InsuranceId = 1, PersonalIdentityNumber = pin, Product = Service.Models.ProductType.Pet, MonthlyCost = 10},
            new() { InsuranceId = 2, PersonalIdentityNumber = pin, Product = Service.Models.ProductType.Car, MonthlyCost = 30, CarRegistrationNumber = "ABC123" }
        };
        _mockInsuranceRepository.Setup(r => r.GetInsurancesByPinAsync(pin)).ReturnsAsync(insuranceEntities);
        _mockVehicleServiceClient.Setup(c => c.GetVehiclesAsync(It.IsAny<string[]>())).ReturnsAsync(new List<VehicleDetails>());

        // Act
        var result = (await _insuranceService.GetInsurancesForPinAsync(pin)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Product.Should().Be(Service.Models.ProductType.Pet);
        result[0].MonthlyCost.Should().Be(10);
        result[1].Product.Should().Be(Service.Models.ProductType.Car);
        result[1].MonthlyCost.Should().Be(30);
    }

    [Fact]
    public async Task GetInsurancesForPinAsync_WithCarInsurance_ShouldCallVehicleServiceAndEnrichData()
    {
        // Arrange
        const string pin = "199001011234";
        const string regNr = "ABC123";
        var insuranceEntities = new Insurance.Service.Models.Insurance[]
        {
            new() { InsuranceId = 1, PersonalIdentityNumber = pin, Product = Service.Models.ProductType.Car, CarRegistrationNumber = regNr }
        };
        var vehicleContracts = new List<VehicleDetails>
        {
            new() { RegistrationNumber = regNr, Make = "Tesla", Model = "Model S" }
        };

        _mockInsuranceRepository.Setup(r => r.GetInsurancesByPinAsync(pin)).ReturnsAsync(insuranceEntities);
        _mockVehicleServiceClient.Setup(c => c.GetVehiclesAsync(It.Is<string[]>(n => n.Contains(regNr)))).ReturnsAsync(vehicleContracts);

        // Act
        var result = (await _insuranceService.GetInsurancesForPinAsync(pin)).ToList();

        // Assert
        _mockVehicleServiceClient.Verify(c => c.GetVehiclesAsync(It.Is<string[]>(n => n.Length == 1 && n[0] == regNr)), Times.Once);
        result.Should().HaveCount(1);
        result[0].VehicleDetails.Should().NotBeNull();
        result[0].VehicleDetails!.Make.Should().Be("Tesla");
    }

    [Fact]
    public async Task GetInsurancesForPinAsync_WithMultipleCarInsurances_ShouldMakeOneBatchCallToVehicleService()
    {
        // Arrange
        const string pin = "199001011234";
        var insuranceEntities = new Insurance.Service.Models.Insurance[]
        {
            new() { InsuranceId = 1, PersonalIdentityNumber = pin, Product = Service.Models.ProductType.Car, CarRegistrationNumber = "CAR1" },
            new() { InsuranceId = 2, PersonalIdentityNumber = pin, Product = Service.Models.ProductType.Car, CarRegistrationNumber = "CAR2" },
            new() { InsuranceId = 3, PersonalIdentityNumber = pin, Product = Service.Models.ProductType.Car, CarRegistrationNumber = "CAR1" } // Duplicate
        };
        _mockInsuranceRepository.Setup(r => r.GetInsurancesByPinAsync(pin)).ReturnsAsync(insuranceEntities);
        _mockVehicleServiceClient.Setup(c => c.GetVehiclesAsync(It.IsAny<string[]>())).ReturnsAsync(new List<VehicleDetails>());

        // Act
        await _insuranceService.GetInsurancesForPinAsync(pin);

        // Assert
        // Verify that the call was made only once with a distinct list of registration numbers
        _mockVehicleServiceClient.Verify(c => c.GetVehiclesAsync(It.Is<string[]>(arr => arr.Length == 2 && arr.Contains("CAR1") && arr.Contains("CAR2"))), Times.Once);
    }

    [Fact]
    public async Task GetInsurancesForPinAsync_WhenVehicleNotFound_ShouldLeaveVehicleDetailsNull()
    {
        // Arrange
        const string pin = "199001011234";
        var insuranceEntities = new Insurance.Service.Models.Insurance[]
        {
            new() { InsuranceId = 1, PersonalIdentityNumber = pin, Product = Service.Models.ProductType.Car, CarRegistrationNumber = "UNKNOWN" }
        };
        _mockInsuranceRepository.Setup(r => r.GetInsurancesByPinAsync(pin)).ReturnsAsync(insuranceEntities);
        // Vehicle service returns an empty list
        _mockVehicleServiceClient.Setup(c => c.GetVehiclesAsync(It.IsAny<string[]>())).ReturnsAsync(new List<VehicleDetails>());

        // Act
        var result = (await _insuranceService.GetInsurancesForPinAsync(pin)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].VehicleDetails.Should().BeNull();
    }

    [Fact]
    public async Task GetInsurancesForPinAsync_WithFeatureToggleDisabled_ShouldNotCallVehicleService()
    {
        // Arrange
        const string pin = "123456789012";
        var insuranceEntities = new Insurance.Service.Models.Insurance[]
        {
            new() { Product = Service.Models.ProductType.Car, PersonalIdentityNumber = pin, CarRegistrationNumber = "ABC123" }
        };
        _mockInsuranceRepository.Setup(r => r.GetInsurancesByPinAsync(pin)).ReturnsAsync(insuranceEntities);
        _mockFeatureToggleService.Setup(f => f.IsVehicleEnrichmentEnabled()).Returns(false);

        // Act
        var result = (await _insuranceService.GetInsurancesForPinAsync(pin)).ToList();

        // Assert
        _mockVehicleServiceClient.Verify(c => c.GetVehiclesAsync(It.IsAny<string[]>()), Times.Never);
        result.First().VehicleDetails.Should().BeNull();
    }
}
