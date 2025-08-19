namespace Vehicle.IntegrationTests.TesHelpers;

public interface ITestDataSeeder
{
    Task InsertVehicleAsync(string registrationNumber, string make);
}