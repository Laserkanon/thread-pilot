using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace Vehicle.Service.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly string _connectionString;

    public VehicleRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")!;
    }

    public async Task<Models.Vehicle?> GetVehicleByRegistrationNumberAsync(string registrationNumber)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql =
            "SELECT RegistrationNumber, Make, Model, ModelYear FROM dbo.Vehicle WHERE RegistrationNumber = @RegistrationNumber";
        return await connection.QuerySingleOrDefaultAsync<Models.Vehicle>(sql,
            new { RegistrationNumber = registrationNumber });
    }

    public async Task<Models.Vehicle[]> GetVehiclesByRegistrationNumbersAsync(string[] registrationNumbers)
    {
        if (registrationNumbers.Length == 0)
            return [];

        await using var connection = new SqlConnection(_connectionString);

        const string sql = @"
        SELECT v.RegistrationNumber, v.Make, v.Model, v.ModelYear
        FROM dbo.Vehicle AS v
        JOIN @RegistrationNumbers AS r ON r.Value = v.RegistrationNumber;";

        var param = new
        {
            RegistrationNumbers = ToStringListTvp(registrationNumbers)
        };

        return (await connection.QueryAsync<Models.Vehicle>(sql, param)).ToArray();
    }

    //Using TVP to avoid in-statement limitation
    private static SqlMapper.ICustomQueryParameter ToStringListTvp(IEnumerable<string> values)
    {
        var table = new DataTable();
        table.Columns.Add("Value", typeof(string));

        foreach (var v in values)
            table.Rows.Add(v);

        return table.AsTableValuedParameter("dbo.StringList");
    }
}