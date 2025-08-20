using FluentValidation;

namespace Vehicle.Service.Validators;

public sealed class RegistrationNumberListValidator : AbstractValidator<string[]>
{
    public RegistrationNumberListValidator()
    {
        RuleFor(x => x).NotNull().WithMessage("registrationNumbers is required.");

        RuleFor(x => x!.Length)
            .GreaterThan(0).WithMessage("Registration number must be greater than zero.");

        RuleForEach(x => x!).SetValidator(new RegistrationNumberValidator());
    }
}