using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Hosting;

public static class SecurityHostingExtensions
{
    public static WebApplicationBuilder AddApiKeyAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthorization()
            .AddAuthentication(ApiKeyAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(ApiKeyAuthHandler.SchemeName, null);
        
        return builder;
    }
}