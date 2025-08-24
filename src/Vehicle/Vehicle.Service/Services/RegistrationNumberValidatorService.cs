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
            if (registrationNumbers.Length == 0)
            {
                return [];
            }

            var duplicates = registrationNumbers
                .GroupBy(r => r)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            if (duplicates.Length != 0)
            {
                _logger.LogWarning("Duplicate registration numbers were found and will be processed only once: {DuplicateNumbers}", 
                    string.Join(", ", duplicates));
            }

            var distinctRegistrationNumbers = registrationNumbers.Distinct();
            var validRegistrationNumbers = new List<string>();
            foreach (var registrationNumber in distinctRegistrationNumbers)
            {
                var validationResult = _validator.Validate(registrationNumber);
                if (validationResult.IsValid)
                {
                    validRegistrationNumbers.Add(registrationNumber);
                }
                else
                {
                    _logger.LogWarning("The registration number {RegistrationNumber} has an invalid format and will be ignored.", registrationNumber);    
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
