
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
    
    public static Models.Insurance MapToModels(this Contracts.Insurance insurance)
    {
        return new Models.Insurance
        {
            InsuranceId = insurance.InsuranceId,
            PersonalIdentityNumber = insurance.PersonalIdentityNumber,
            Product = (Models.ProductType)insurance.Product,
            MonthlyCost = insurance.MonthlyCost,
            CarRegistrationNumber = insurance.CarRegistrationNumber,
            VehicleDetails = insurance.VehicleDetails?.MapToModels()
        };
    }

    private static VehicleDetails MapToContracts(this Models.VehicleDetails vehicleDetails)
    {
        return new VehicleDetails
        {
            Make = vehicleDetails.Make,
            Model = vehicleDetails.Model,
            ModelYear = vehicleDetails.ModelYear,
            RegistrationNumber = vehicleDetails.RegistrationNumber,
        };
    }
    
    private static Models.VehicleDetails MapToModels(this VehicleDetails vehicleDetails)
    {
        return new Models.VehicleDetails
        {
            Make = vehicleDetails.Make,
            Model = vehicleDetails.Model,
            ModelYear = vehicleDetails.ModelYear,
            RegistrationNumber = vehicleDetails.RegistrationNumber,
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