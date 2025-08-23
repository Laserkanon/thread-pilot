using Insurance.Service.Models;

namespace Insurance.Service.Clients;

public interface IVehicleServiceClient
{
    Task<IEnumerable<VehicleDetails>> GetVehiclesBatchAsync(string[] registrationNumbers);
    Task<IEnumerable<VehicleDetails>> GetVehiclesConcurrentlyAsync(string[] registrationNumbers);
}
