using FluentValidation;

namespace Vehicle.Service.Services
{
    public class RegistrationNumberValidatorService : IRegistrationNumberValidatorService
    {
        private readonly IValidator<string> _validator;
        private readonly ILogger<RegistrationNumberValidatorService> _logger;

        public RegistrationNumberValidatorService(IValidator<string> validator, ILogger<RegistrationNumberValidatorService> logger)
        {
            _validator = validator;
            _logger = logger;
        }

        public string[] Validate(string[] registrationNumbers)
        {
            if (registrationNumbers == null || registrationNumbers.Length == 0)
            {
                return Array.Empty<string>();
            }

            var validRegistrationNumbers = new List<string>();
            foreach (var registrationNumber in registrationNumbers)
            {
                var validationResult = _validator.Validate(registrationNumber);
                if (validationResult.IsValid)
                {
                    validRegistrationNumbers.Add(registrationNumber);
                }
            }

            if (validRegistrationNumbers.Count > 500)
            {
                _logger.LogWarning("The number of registration numbers exceeds the limit of 500. The list will be truncated.");
                return validRegistrationNumbers.Take(500).ToArray();
            }

            return validRegistrationNumbers.ToArray();
        }
    }
}
