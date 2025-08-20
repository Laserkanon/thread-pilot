namespace Vehicle.Service.Services
{
    public interface IRegistrationNumberValidatorService
    {
        string[] Validate(string[] registrationNumbers);
    }
}
