using Infrastructure.HttpClient;

namespace Insurance.Service.Configuration;

public class VehicleServiceClientConfiguration : IHttpClientBaseConfiguration
{
    public required string BaseUrl { get; set; }
    public required string ApiKey { get; set; } 
    public int MaxDegreeOfParallelismSingle { get; set; }
    public int MaxBatchSize { get; set; }
    public int MaxDegreeOfParallelismBatch { get; set; }
}
