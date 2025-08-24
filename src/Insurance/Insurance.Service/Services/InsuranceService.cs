using Infrastructure.FeatureToggle;
using Insurance.Service.Clients;
using Insurance.Service.Models;
using Insurance.Service.Repositories;
using Insurance.Service.Settings;

namespace Insurance.Service.Services;

public class InsuranceService : IInsuranceService
{
    private readonly IInsuranceRepository _insuranceRepository;
    private readonly IVehicleServiceClient _vehicleServiceClient;
    private readonly IFeatureToggleService<InsuranceFeatureToggles> _featureToggleService;

    public InsuranceService(
        IInsuranceRepository insuranceRepository,
        IVehicleServiceClient vehicleServiceClient,
        IFeatureToggleService<InsuranceFeatureToggles> featureToggleService)
    {
        _insuranceRepository = insuranceRepository;
        _vehicleServiceClient = vehicleServiceClient;
        _featureToggleService = featureToggleService;
    }

    public async Task<IEnumerable<Models.Insurance>> GetInsurancesForPinAsync(string personalIdentityNumber)
    {
        var insuranceEntities = await _insuranceRepository.GetInsurancesByPinAsync(personalIdentityNumber);

        if (insuranceEntities.Length == 0)
        {
            return [];
        }

        if (_featureToggleService.Toggles.EnableVehicleEnrichment)
        {
            await EnrichInsurancesWithVehicleDetailsAsync(insuranceEntities);
        }

        return insuranceEntities;
    }

    private async Task EnrichInsurancesWithVehicleDetailsAsync(IEnumerable<Models.Insurance> insurances)
    {
        var carInsurances = insurances
            .Where(i => i.Product == ProductType.Car && !string.IsNullOrEmpty(i.CarRegistrationNumber))
            .ToList();

        if (carInsurances.Count == 0)
        {
            return;
        }

        var registrationNumbers = carInsurances
            .Select(i => i.CarRegistrationNumber!)
            .Distinct() //No need to fetch duplicates
            .ToArray();

        var vehicleDetails = _featureToggleService.Toggles.EnableBatchVehicleCall
            ? await _vehicleServiceClient.GetVehiclesBatchAsync(registrationNumbers)
            : await _vehicleServiceClient.GetVehiclesConcurrentlyAsync(registrationNumbers);
        
        var vehicleDict = vehicleDetails.ToDictionary(v => v.RegistrationNumber);

        foreach (var insurance in carInsurances)
        {
            if (vehicleDict.TryGetValue(insurance.CarRegistrationNumber!, out var vehicle))
            {
                insurance.VehicleDetails = vehicle;
            }
        }
    }
}
