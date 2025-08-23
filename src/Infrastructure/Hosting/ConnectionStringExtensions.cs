using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Hosting;

public static class ConnectionStringExtensions
{
    /// <summary>
    /// Reads "ConnectionStrings:Default" and overwrites its password with "DB_SA_PASSWORD".
    /// Throws if either is missing. No fallbacks, no Integrated Security handling.
    /// </summary>
    public static string RequireSqlConnectionString(this IConfiguration config)
    {
        var cs  = config.GetConnectionString("Default")
                  ?? throw new InvalidOperationException("Missing 'ConnectionStrings:Default'.");

        var pwd = config["DB_SA_PASSWORD"]
                  ?? throw new InvalidOperationException("Missing 'DB_SA_PASSWORD'.");

        var builder = new SqlConnectionStringBuilder(cs) { Password = pwd };

        return builder.ConnectionString;
    }
}