namespace Infrastructure.HttpClient;

public interface IHttpClientBaseConfiguration
{
    string BaseUrl { get; set; }
    string ApiKey { get; set; }
}
