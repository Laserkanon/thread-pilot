
using Insurance.Service.Contracts;

namespace Insurance.Service.Extensions;

public static class MapperExtensions
{
    public static Contracts.Insurance MapToContracts(this Models.Insurance insurance)
    {
        return new Insurance.Service.Contracts.Insurance
        {
            InsuranceId = insurance.InsuranceId,
            PersonalIdentityNumber = insurance.PersonalIdentityNumber,
            Product = (ProductType)insurance.Product,
            MonthlyCost = insurance.MonthlyCost,
            CarRegistrationNumber = insurance.CarRegistrationNumber,
            VehicleDetails = insurance.VehicleDetails?.MapToContracts()
        };
    }
    
    private static VehicleDetails MapToContracts(this Models.VehicleDetails vehicleDetails)
    {
        return new VehicleDetails
        {
            Make = vehicleDetails.Make,
            Model = vehicleDetails.Model,
            ModelYear = vehicleDetails.ModelYear
        };
    }

    public static Models.VehicleDetails MapToModels(this Vehicle.Service.Contracts.Vehicle vehicle)
    {
        return new Models.VehicleDetails
        {
            Make = vehicle.Make,
            Model = vehicle.Model,
            ModelYear = vehicle.ModelYear,
            RegistrationNumber = vehicle.RegistrationNumber,
        };
    }
}