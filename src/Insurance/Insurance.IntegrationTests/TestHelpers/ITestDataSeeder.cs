using Insurance.Service.Contracts;

namespace Insurance.IntegrationTests.TestHelpers;

public interface ITestDataSeeder
{
    Task InsertInsuranceAsync(string personalIdentityNumber, ProductType product, decimal monthlyCost, string? carRegistrationNumber = null);
}