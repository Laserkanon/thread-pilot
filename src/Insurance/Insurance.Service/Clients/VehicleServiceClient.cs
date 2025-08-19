using System.Net;
using Insurance.Service.Extensions;

namespace Insurance.Service.Clients;

public class VehicleServiceClient : IVehicleServiceClient
{
    private readonly HttpClient _httpClient;

    public VehicleServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        var baseUrl = configuration.GetValue<string>("VehicleService:BaseUrl")
            ?? throw new InvalidOperationException("VehicleService:BaseUrl is not configured.");

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<IEnumerable<Models.VehicleDetails>> GetVehiclesAsync(string[] registrationNumbers)
    {
        if (registrationNumbers.Length == 0)
        {
            return [];
        }

        var response = await _httpClient.PostAsJsonAsync("/api/v1/vehicles/batch", registrationNumbers);
        
        response.EnsureSuccessStatusCode();

        var vehicles = await response.Content.ReadFromJsonAsync<Vehicle.Service.Contracts.Vehicle[]>();
        
        if(vehicles == null || vehicles.Length == 0)
            return Array.Empty<Models.VehicleDetails>();

        return vehicles.Select(x => x.MapToModels());
    }
}
