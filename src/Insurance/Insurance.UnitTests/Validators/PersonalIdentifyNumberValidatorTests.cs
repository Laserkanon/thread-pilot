using FluentAssertions;
using Insurance.Service.Validators;

namespace Insurance.UnitTests.Validators;

public class PersonalIdentifyNumberValidatorTests
{
    private readonly PersonalIdentifyNumberValidator _validator = new();

    [Theory]
    [InlineData("123456789012", true)]  // Valid
    [InlineData("12345678901", false)]  // Too short
    [InlineData("1234567890123", false)] // Too long
    [InlineData("abcdefghijkl", false)] // Invalid characters
    [InlineData("12345678901a", false)] // Invalid characters
    [InlineData("", false)]             // Empty
    public void PinValidation_ShouldReturnExpectedResult(string pin, bool expectedIsValid)
    {
        // Act
        var result = _validator.Validate(pin);

        // Assert
        result.IsValid.Should().Be(expectedIsValid);
    }
}
