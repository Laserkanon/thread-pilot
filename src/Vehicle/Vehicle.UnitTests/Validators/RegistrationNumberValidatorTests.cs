using FluentValidation.TestHelper;
using Vehicle.Service.Validators;

namespace Vehicle.UnitTests.Validators;

public class RegistrationNumberValidatorTests
{
    private readonly RegistrationNumberValidator _sut = new();

    [Theory]
    [InlineData("AB")]        // min length
    [InlineData("ABC1234")]   // max length (7)
    [InlineData("ZZZZZZZ")]
    public void Valid_Values_Pass(string input)
    {
        var result = _sut.TestValidate(input);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]           //empty
    public void Empty_Value_Fails(string? input)
    {
        var result = _sut.TestValidate(input!);
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Registration number is required.");
    }

    [Theory]
    [InlineData("A")]          // too short
    [InlineData("ABCDEFGH")]   // length 8 (too long)
    
    public void Wrong_Values_Fails(string input)
    {
        var result = _sut.TestValidate(input);
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Registration number must be between 2 and 7 characters.");
    }
}