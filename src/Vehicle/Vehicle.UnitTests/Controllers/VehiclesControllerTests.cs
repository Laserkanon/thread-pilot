using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Vehicle.Service.Controllers;
using Vehicle.Service.Repositories;
using Vehicle.Service.Services;

namespace Vehicle.UnitTests.Controllers
{
    public class VehiclesControllerTests
    {
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock = new();
        private readonly Mock<IValidator<string>> _regValidatorMock = new();
        private readonly Mock<IRegistrationNumberValidatorService> _validatorServiceMock = new();
        private readonly VehiclesController _sut;

        public VehiclesControllerTests()
        {
            _sut = new VehiclesController(
                _vehicleRepositoryMock.Object,
                _regValidatorMock.Object,
                _validatorServiceMock.Object);
        }

        [Fact]
        public async Task GetVehiclesBatch_WithValidRegistrationNumbers_ReturnsOk()
        {
            // Arrange
            var registrationNumbers = new[] { "ABC-123", "DEF-456" };
            var validRegistrationNumbers = new[] { "ABC-123", "DEF-456" };
            var vehicles = new Service.Models.Vehicle[]
            {
                new() { RegistrationNumber = "ABC-123" },
                new() { RegistrationNumber = "DEF-456" }
            };

            _validatorServiceMock.Setup(x => x.Validate(registrationNumbers)).Returns(validRegistrationNumbers);
            _vehicleRepositoryMock.Setup(x => x.GetVehiclesByRegistrationNumbersAsync(validRegistrationNumbers)).ReturnsAsync(vehicles);

            // Act
            var result = await _sut.GetVehiclesBatch(registrationNumbers);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedVehicles = Assert.IsAssignableFrom<IEnumerable<Vehicle.Service.Contracts.Vehicle>>(okResult.Value);
            Assert.Equal(2, returnedVehicles.Count());
        }

        [Fact]
        public async Task GetVehiclesBatch_WithNoValidRegistrationNumbers_ReturnsOkWithEmptyList()
        {
            // Arrange
            var registrationNumbers = new[] { "A", "B" };
            var validRegistrationNumbers = Array.Empty<string>();
            var vehicles = Array.Empty<Service.Models.Vehicle>();

            _validatorServiceMock.Setup(x => x.Validate(registrationNumbers)).Returns(validRegistrationNumbers);
            _vehicleRepositoryMock.Setup(x => x.GetVehiclesByRegistrationNumbersAsync(validRegistrationNumbers)).ReturnsAsync(vehicles);


            // Act
            var result = await _sut.GetVehiclesBatch(registrationNumbers);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedVehicles = Assert.IsType<IEnumerable<Vehicle.Service.Contracts.Vehicle>>(okResult.Value, exactMatch: false);
            Assert.Empty(returnedVehicles);
        }

    }
}
