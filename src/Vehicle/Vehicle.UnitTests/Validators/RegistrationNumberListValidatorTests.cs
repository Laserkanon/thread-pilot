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
    public void Invalid_Item_Is_Indexed()
    {
        var payload = new[] { "AB", "A" }; // second item too short
        var result = _sut.TestValidate(payload);

        // NOTE: For arrays, the property path is "x[1]" (includes the lambda param).
        result.ShouldHaveValidationErrorFor("x[1]")
            .WithErrorMessage("Registration number must be between 2 and 7 characters.");
    }

    [Fact]
    public void Exceeds_Max_Batch_Size_Fails()
    {
        var over = Enumerable.Repeat("AB", RegistrationNumberListValidator.MaxBatchSize + 1).ToArray();
        var result = _sut.TestValidate(over);

        result.ShouldHaveValidationErrorFor(x => x.Length)
            .WithErrorMessage($"At most {RegistrationNumberListValidator.MaxBatchSize} registration numbers are allowed.");
    }
}