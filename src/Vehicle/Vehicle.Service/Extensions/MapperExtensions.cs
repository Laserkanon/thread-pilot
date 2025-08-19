
namespace Vehicle.Service.Extensions;

public static class MapperExtensions
{
    public static Contracts.Vehicle MapToContracts(this Models.Vehicle vehicle)
    {
        return new Contracts.Vehicle
        {
            RegistrationNumber = vehicle.RegistrationNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            ModelYear = vehicle.ModelYear
        };
    }
}