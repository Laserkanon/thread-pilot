using Insurance.Service.Contracts.Settings;

namespace Insurance.Service.Contracts;

public class VehicleServiceClientConfiguration : IClientConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int MaxDegreeOfParallelismSingle { get; set; }
    public int MaxBatchSize { get; set; }
    public int MaxDegreeOfParallelismBatch { get; set; }
}
