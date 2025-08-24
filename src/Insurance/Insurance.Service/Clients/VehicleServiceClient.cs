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
    private readonly int _maxBatchSize;
    private readonly int _maxParallelBatches;

    public VehicleServiceClient(
        HttpClient httpClient,
        ILogger<VehicleServiceClient> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _maxDegreeOfParallelism = configuration.GetValue("Vehicle.Service.Client:MaxDegreeOfParallelism", 5);
        _maxBatchSize = configuration.GetValue("Vehicle.Service.Client:MaxBatchSize", 50);
        _maxParallelBatches = configuration.GetValue("Vehicle.Service.Client:MaxParallelBatches", 1);
    }

    public async Task<IEnumerable<Models.VehicleDetails>> GetVehiclesBatchAsync(string[] registrationNumbers)
    {
        if (registrationNumbers.Length == 0)
        {
            return [];
        }

        var chunks = registrationNumbers.Chunk(_maxBatchSize);
        var allVehicles = new System.Collections.Concurrent.ConcurrentBag<Models.VehicleDetails>();

        await Parallel.ForEachAsync(chunks, new ParallelOptions { MaxDegreeOfParallelism = _maxParallelBatches }, async (chunk, _) =>
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/vehicles/batch", chunk);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("A non-successful status code {StatusCode} was received from the Vehicle Service. RegistrationNumbers: {RegistrationNumbers}", response.StatusCode, string.Join(", ", chunk));
                return;
            }

            var vehicles = await response.Content.ReadFromJsonAsync<Vehicle.Service.Contracts.Vehicle[]>();

            if (vehicles != null && vehicles.Length > 0)
            {
                foreach(var vehicle in vehicles)
                {
                    allVehicles.Add(vehicle.MapToModels());
                }
            }
        });

        return allVehicles;
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
