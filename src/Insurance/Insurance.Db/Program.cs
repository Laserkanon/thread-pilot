using DbUp;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);
var config = builder.Build();

var connectionString = config.GetConnectionString("Default")
                    ?? config["connection"]
                    ?? throw new InvalidOperationException("No connection string. Set ConnectionStrings:Default or pass --connection=<...>");

EnsureDatabase.For.SqlDatabase(connectionString);

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
return 0;
