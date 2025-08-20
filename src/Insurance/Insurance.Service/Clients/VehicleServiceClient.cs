using System.Net;
using System.Text.Json;
using Insurance.Service.Extensions;

namespace Insurance.Service.Clients;

public class VehicleServiceClient : IVehicleServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleServiceClient> _logger;

    public VehicleServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<VehicleServiceClient> logger)
    {
        var baseUrl = configuration.GetValue<string>("VehicleService:BaseUrl")
            ?? throw new InvalidOperationException("VehicleService:BaseUrl is not configured.");

        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<IEnumerable<Models.VehicleDetails>> GetVehiclesAsync(string[] registrationNumbers)
    {
        if (registrationNumbers.Length == 0)
        {
            return [];
        }

        var response = await _httpClient.PostAsJsonAsync("/api/v1/vehicles/batch", registrationNumbers);

        //TODO: Consider adding partial not found identification of batch entries
        // Potential data discrepancy  
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("One or more registrations were not found. RegistrationNumbers: {RegistrationNumbers}", registrationNumbers);
            return [];
        }

        //TODO: Consider adding partial bad request handling of batch entries
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("One or more registrations were not in the correct format. RegistrationNumbers: {RegistrationNumbers}", registrationNumbers);
            return [];
        }

        //TODO: Consider fallback strategy if service is down.
        response.EnsureSuccessStatusCode();

        var vehicles = await response.Content.ReadFromJsonAsync<Vehicle.Service.Contracts.Vehicle[]>();
        
        if(vehicles == null || vehicles.Length == 0)
            return Array.Empty<Models.VehicleDetails>();

        return vehicles.Select(x => x.MapToModels());
    }
}
