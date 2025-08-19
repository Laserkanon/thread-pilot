using FluentValidation;

namespace Vehicle.Service.Validators;

public class RegistrationNumberValidator : AbstractValidator<string>
{
    private const int MaxLength = 7;
    private const int MinLength = 2;

    public RegistrationNumberValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("Registration number is required.")
            .Must(s => s!.Length >= MinLength && s.Length <= MaxLength)
            .WithMessage($"Registration number must be between {MinLength} and {MaxLength} characters.");
    }
}