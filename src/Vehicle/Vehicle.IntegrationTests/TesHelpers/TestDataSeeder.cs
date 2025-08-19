using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Vehicle.IntegrationTests.TesHelpers;

public sealed class TestDataSeeder : ITestDataSeeder
{
    private readonly string _cs;

    public TestDataSeeder(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("Default")
              ?? throw new InvalidOperationException("ConnectionStrings:Default not configured");
    }

    public async Task InsertVehicleAsync(string registrationNumber, string make)
    {
        const string sql = @"
INSERT INTO dbo.Vehicle (RegistrationNumber, Make)
VALUES (@RegistrationNumber, @Make);";

        await using var conn = new SqlConnection(_cs);
        await conn.ExecuteAsync(sql, new { RegistrationNumber = registrationNumber, Make = make });
    }
}