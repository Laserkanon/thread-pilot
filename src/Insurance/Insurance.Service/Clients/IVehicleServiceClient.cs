using Insurance.Service.Models;

namespace Insurance.Service.Clients;

public interface IVehicleServiceClient
{
    Task<IEnumerable<VehicleDetails>> GetVehiclesAsync(string[] registrationNumbers);
}
