using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Insurance.IntegrationTests.TestHelpers;

public static class HttpClientExtensions
{
    public static HttpClient WithBearerToken(this HttpClient client, string key, string issuer, string audience, IEnumerable<string> scopes)
    {
        var token = TestJwtTokenGenerator.GenerateJwtToken(key, issuer, audience, scopes);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
