using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Vehicle.Service.Services;
using Vehicle.Service.Validators;
using Xunit;

namespace Vehicle.UnitTests.Services
{
    public class RegistrationNumberValidatorServiceTests
    {
        private readonly RegistrationNumberValidatorService _sut;
        private readonly RegistrationNumberValidator _validator = new();
        private readonly Mock<ILogger<RegistrationNumberValidatorService>> _loggerMock = new();

        public RegistrationNumberValidatorServiceTests()
        {
            _sut = new RegistrationNumberValidatorService(_validator, _loggerMock.Object);
        }

        [Fact]
        public void Validate_WithValidRegistrationNumbers_ReturnsAll()
        {
            // Arrange
            var registrationNumbers = new[] { "ABC-123", "DEF-456" };

            // Act
            var result = _sut.Validate(registrationNumbers);

            // Assert
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void Validate_WithInvalidRegistrationNumbers_ReturnsEmpty()
        {
            // Arrange
            var registrationNumbers = new[] { "A", "B" };

            // Act
            var result = _sut.Validate(registrationNumbers);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_WithMixedRegistrationNumbers_ReturnsOnlyValid()
        {
            // Arrange
            var registrationNumbers = new[] { "ABC-123", "A", "DEF-456", "B" };

            // Act
            var result = _sut.Validate(registrationNumbers);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Contains("ABC-123", result);
            Assert.Contains("DEF-456", result);
        }

        [Fact]
        public void Validate_WithEmptyList_ReturnsEmpty()
        {
            // Arrange
            var registrationNumbers = Array.Empty<string>();

            // Act
            var result = _sut.Validate(registrationNumbers);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_WithMoreThan500RegistrationNumbers_TruncatesList()
        {
            // Arrange
            var registrationNumbers = Enumerable.Range(1, 501).Select(_ => $"ABC-123").ToArray();

            // Act
            var result = _sut.Validate(registrationNumbers);

            // Assert
            Assert.Equal(500, result.Length);
        }

        [Fact]
        public void Validate_WithMoreThan500RegistrationNumbers_LogsWarning()
        {
            // Arrange
            var registrationNumbers = Enumerable.Range(1, 501).Select(_ => $"ABC-123").ToArray();

            // Act
            _sut.Validate(registrationNumbers);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("The number of registration numbers exceeds the limit of 500.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }
    }
}
