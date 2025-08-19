using Dapper;
using Microsoft.Data.SqlClient;

namespace Insurance.Service.Repositories;

public class InsuranceRepository : IInsuranceRepository
{
    private readonly string _connectionString;

    public InsuranceRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")!;
    }

    public async Task<Models.Insurance[]> GetInsurancesByPinAsync(string personalIdentityNumber)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT InsuranceId, PersonalIdentityNumber, Product, CarRegistrationNumber, MonthlyCost
            FROM dbo.Insurance
            WHERE PersonalIdentityNumber = @PersonalIdentityNumber";

        return (await connection.QueryAsync<Models.Insurance>(sql, new { PersonalIdentityNumber = personalIdentityNumber })).ToArray();
    }
}
