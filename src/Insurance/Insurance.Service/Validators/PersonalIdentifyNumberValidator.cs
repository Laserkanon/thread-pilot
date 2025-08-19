using FluentValidation;

namespace Insurance.Service.Validators;

//TODO: Add a more comprehensive personal identity number validation
public class PersonalIdentifyNumberValidator : AbstractValidator<string?>
{
    public PersonalIdentifyNumberValidator()
    {
        RuleFor(pin => pin)
            .NotEmpty()
            .Length(12).WithMessage("Personal identity number must be exactly 12 characters.")
            .Matches("^[0-9]{12}$").WithMessage("Personal identity number must contain only digits.");
    }
}
