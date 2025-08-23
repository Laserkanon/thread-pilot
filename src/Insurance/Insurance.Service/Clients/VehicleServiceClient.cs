using System.Net;
using System.Text.Json;
using Insurance.Service.Extensions;
using Insurance.Service.Services;

namespace Insurance.Service.Clients;

public class VehicleServiceClient : IVehicleServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleServiceClient> _logger;
    private readonly int _maxDegreeOfParallelism;

    public VehicleServiceClient(
        HttpClient httpClient,
        ILogger<VehicleServiceClient> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _maxDegreeOfParallelism = configuration.GetValue("Vehicle.Service.Client:MaxDegreeOfParallelism", 5);
    }

    public async Task<IEnumerable<Models.VehicleDetails>> GetVehiclesBatchAsync(string[] registrationNumbers)
    {
        if (registrationNumbers.Length == 0)
        {
            return [];
        }

        var response = await _httpClient.PostAsJsonAsync("/api/v1/vehicles/batch", registrationNumbers);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("A non-successful status code {StatusCode} was received from the Vehicle Service. RegistrationNumbers: {RegistrationNumbers}", response.StatusCode, registrationNumbers);
            return [];
        }

        var vehicles = await response.Content.ReadFromJsonAsync<Vehicle.Service.Contracts.Vehicle[]>();
        
        if(vehicles == null || vehicles.Length == 0)
            return Array.Empty<Models.VehicleDetails>();

        return vehicles.Select(x => x.MapToModels());
    }

    public async Task<IEnumerable<Models.VehicleDetails>> GetVehiclesConcurrentlyAsync(string[] registrationNumbers)
    {
        var vehicleDetails = new System.Collections.Concurrent.ConcurrentBag<Models.VehicleDetails>();

        await Parallel.ForEachAsync(registrationNumbers,
            new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
            async (regNumber, _) =>
            {
                var vehicle = await GetVehicleAsync(regNumber);
                if (vehicle != null)
                {
                    vehicleDetails.Add(vehicle);
                }
            });

        return vehicleDetails.ToList();
    }

    private async Task<Models.VehicleDetails?> GetVehicleAsync(string registrationNumber)
    {
        var response = await _httpClient.GetAsync($"/api/v1/vehicles/{registrationNumber}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("A non-successful status code {StatusCode} was received from the Vehicle Service. RegistrationNumber: {RegistrationNumber}", response.StatusCode, registrationNumber);
            return null;
        }

        var vehicle = await response.Content.ReadFromJsonAsync<Vehicle.Service.Contracts.Vehicle>();

        return vehicle?.MapToModels();
    }
}
