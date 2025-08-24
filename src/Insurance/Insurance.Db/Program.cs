using DbUp;
using Infrastructure.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

const int maxWaitTimeSeconds = 30;
const int retryIntervalSeconds = 5;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddCommandLine(args);
var config = builder.Build();

var connectionString = config.RequireSqlConnectionString();

// A list of transient error codes that are safe to retry.
// This list can be expanded based on SQL Server documentation for transient faults.
var transientErrorNumbers = new[]
{
    10054, // A connection was successfully established with the server, but then an error occurred during the pre-login handshake.
    18456  // Login failed for user. (Can happen when SQL Server is starting up)
};

var stopwatch = Stopwatch.StartNew();
while (stopwatch.Elapsed.TotalSeconds < maxWaitTimeSeconds)
{
    try
    {
        Console.WriteLine("Attempting to connect to the database...");
        EnsureDatabase.For.SqlDatabase(connectionString);
        Console.WriteLine("Database connection successful. Running migrations...");

        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .JournalToSqlTable("dbo", "_SchemaVersions")
            .WithExecutionTimeout(TimeSpan.FromSeconds(120))
            .WithTransactionPerScript()
            .LogToConsole()
            .WithScriptsFromFileSystem(Path.Combine(AppContext.BaseDirectory, "Migrations", "Schema"))
            .WithScriptsFromFileSystem(Path.Combine(AppContext.BaseDirectory, "Migrations", "Data"))
            .Build();

        var schemaResult = upgrader.PerformUpgrade();

        if (!schemaResult.Successful)
        {
            Console.Error.WriteLine(schemaResult.Error);
            return -1;
        }

        Console.WriteLine("DbUp completed successfully.");
        return 0; // Success!
    }
    catch (SqlException ex)
    {
        // Check if the error is in our list of known transient errors.
        if (transientErrorNumbers.Contains(ex.Number))
        {
            // This is a temporary connection issue, so we wait and retry.
            Console.WriteLine($"Database not ready (Error: {ex.Number}). Retrying in {retryIntervalSeconds} seconds... ({Math.Round(maxWaitTimeSeconds - stopwatch.Elapsed.TotalSeconds)}s remaining)");
            Thread.Sleep(retryIntervalSeconds * 1000);
        }
        else
        {
            // This is a different SQL error (e.g., syntax error). We should fail immediately.
            Console.Error.WriteLine($"❌ Unrecoverable SQL error occurred (Error: {ex.Number}): {ex.Message}");
            throw; // Re-throw the exception to stop the application.
        }
    }
}

// If the loop completes without returning, it means we timed out
Console.Error.WriteLine("❌ Timed out: Failed to connect to the database after several retries.");
return -1;