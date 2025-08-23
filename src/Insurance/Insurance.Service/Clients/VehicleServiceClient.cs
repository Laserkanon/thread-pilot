using System.Net;
using System.Text.Json;
using Insurance.Service.Extensions;

namespace Insurance.Service.Clients;

public class VehicleServiceClient : IVehicleServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleServiceClient> _logger;

    public VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<Models.VehicleDetails>> GetVehiclesAsync(string[] registrationNumbers)
    {
        if (registrationNumbers.Length == 0)
        {
            return [];
        }

        var response = await _httpClient.PostAsJsonAsync("/api/v1/vehicles/batch", registrationNumbers);
        
        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("A non-successful status code {StatusCode} was received from the Vehicle Service. RegistrationNumbers: {RegistrationNumbers}", response.StatusCode, registrationNumbers);
            return [];
        }

        response.EnsureSuccessStatusCode();

        var vehicles = await response.Content.ReadFromJsonAsync<Vehicle.Service.Contracts.Vehicle[]>();
        
        if(vehicles == null || vehicles.Length == 0)
            return Array.Empty<Models.VehicleDetails>();

        return vehicles.Select(x => x.MapToModels());
    }
}
