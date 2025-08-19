namespace Insurance.Service.Models;

public class VehicleDetails
{
    public required string RegistrationNumber { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? ModelYear { get; set; }
}
