using Infrastructure.Hosting;
using Insurance.Service.Contracts;
using Microsoft.Extensions.Configuration;

namespace Insurance.IntegrationTests.TestHelpers;

public sealed class TestDataSeeder : ITestDataSeeder
{
    private readonly string _cs;
    public TestDataSeeder(IConfiguration cfg)
    {
        _cs = cfg.RequireSqlConnectionString()
              ?? throw new InvalidOperationException("ConnectionStrings:Default not configured");
    }

    public async Task InsertInsuranceAsync(string personalIdentityNumber, ProductType product, decimal monthlyCost, string? carRegistrationNumber = null)
    {
        const string sql = @"
            INSERT INTO dbo.Insurance (PersonalIdentityNumber, Product, MonthlyCost, CarRegistrationNumber)
            VALUES (@PersonalIdentityNumber, @Product, @MonthlyCost, @CarRegistrationNumber);";

        await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_cs);
        await Dapper.SqlMapper.ExecuteAsync(conn, sql, new
        {
            PersonalIdentityNumber = personalIdentityNumber,
            Product = (short)product,
            MonthlyCost = monthlyCost,
            CarRegistrationNumber = carRegistrationNumber
        });
    }
}