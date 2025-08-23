using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Infrastructure.Hosting;

/// <summary>
/// Minimum serilog
/// </summary>
public static class SerilogHostingExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(
        this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((ctx, services, cfg) => cfg
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console());

        return builder;
    }

    public static WebApplication UseSerilogDefaults(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        return app;
    }
}