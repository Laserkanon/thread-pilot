using Insurance.Service.Clients;
using Insurance.Service.Models;
using Insurance.Service.Repositories;

namespace Insurance.Service.Services;

public class InsuranceService : IInsuranceService
{
    private readonly IInsuranceRepository _insuranceRepository;
    private readonly IVehicleServiceClient _vehicleServiceClient;
    private readonly IFeatureToggleService _featureToggleService;

    public InsuranceService(
        IInsuranceRepository insuranceRepository,
        IVehicleServiceClient vehicleServiceClient,
        IFeatureToggleService featureToggleService)
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

        if (_featureToggleService.IsVehicleEnrichmentEnabled())
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
            .Distinct()
            .ToArray();

        var vehicleDetails = await _vehicleServiceClient.GetVehiclesAsync(registrationNumbers);
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
