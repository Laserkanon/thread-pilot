namespace Vehicle.Service.Repositories;

public interface IVehicleRepository
{
    Task<Models.Vehicle?> GetVehicleByRegistrationNumberAsync(string registrationNumber);
    Task<Models.Vehicle[]> GetVehiclesByRegistrationNumbersAsync(string[] registrationNumbers);
}
