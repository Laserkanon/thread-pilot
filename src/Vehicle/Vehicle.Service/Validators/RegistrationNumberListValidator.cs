using FluentValidation;

namespace Vehicle.Service.Validators;

public sealed class RegistrationNumberListValidator : AbstractValidator<string[]>
{
    public const int MaxBatchSize = 100;

    public RegistrationNumberListValidator()
    {
        RuleFor(x => x).NotNull().WithMessage("registrationNumbers is required.");

        RuleFor(x => x!.Length)
            .GreaterThan(0).WithMessage("Registration number must be greater than zero.")
            .LessThanOrEqualTo(MaxBatchSize)
            .WithMessage($"At most {MaxBatchSize} registration numbers are allowed.");

        RuleForEach(x => x!).SetValidator(new RegistrationNumberValidator());
    }
}