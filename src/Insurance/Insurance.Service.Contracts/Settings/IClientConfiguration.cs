namespace Insurance.Service.Contracts.Settings;

public interface IClientConfiguration
{
    string BaseUrl { get; set; }
    string ApiKey { get; set; }
}
