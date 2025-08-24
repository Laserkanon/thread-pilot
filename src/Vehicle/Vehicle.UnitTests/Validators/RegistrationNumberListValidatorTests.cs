using FluentValidation.TestHelper;
using Vehicle.Service.Validators;

namespace Vehicle.UnitTests.Validators;

public class RegistrationNumberListValidatorTests
{
    private readonly RegistrationNumberListValidator _sut = new();

    [Fact]
    public void Empty_List_Fails()
    {
        var result = _sut.TestValidate([]);
        result.ShouldHaveValidationErrorFor(x => x.Length)
            .WithErrorMessage("Registration number must be greater than zero.");
    }

    [Fact]
    public void Valid_List_Passes()
    {
        var payload = new[] { "AB", "ABC1234" }; // 2..7 chars
        var result = _sut.TestValidate(payload);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Item_Too_Short_Is_Indexed()
    {
        var payload = new[] { "AB", "A" }; // second item too short
        var result = _sut.TestValidate(payload);

        // NOTE: For arrays, the property path is "x[1]" (includes the lambda param).
        result.ShouldHaveValidationErrorFor("x[1]")
            .WithErrorMessage("Registration number must be between 2 and 7 characters.");
    }

    [Fact]
    public void Item_Too_Long_Is_Indexed()
    {
        var payload = new[] { "ABC", "ABC12345" }; // second item is 8 chars long
        var result = _sut.TestValidate(payload);

        result.ShouldHaveValidationErrorFor("x[1]")
            .WithErrorMessage("Registration number must be between 2 and 7 characters.");
    }

    [Fact]
    public void List_With_Exactly_500_Items_Passes()
    {
        // Arrange: Create a list with exactly 500 valid items
        var payload = Enumerable.Range(1, 500)
            .Select(i => $"REG{i:D3}") // e.g., REG001
            .ToArray();
        
        // Act
        var result = _sut.TestValidate(payload);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}