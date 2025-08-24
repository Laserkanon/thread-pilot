using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Hosting;

public static class PipelineHostingExtensions
{
    public static WebApplication UseCommonPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //TLS termination
        var disableHttpsRedirection = app.Configuration.GetValue<bool>("DISABLE_HTTPS_REDIRECTION");
        if (!disableHttpsRedirection)
        {
            app.UseHttpsRedirection();
        }

        app.MapPrometheusScrapingEndpoint();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/healthz").WithMetadata(new AllowAnonymousAttribute());

        return app;
    }
}