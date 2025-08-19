namespace Insurance.Service.Models;

public class Insurance
{
    public long InsuranceId { get; set; }

    public required string PersonalIdentityNumber { get; set; }

    public ProductType Product { get; set; }
    
    public decimal MonthlyCost { get; set; }

    public string? CarRegistrationNumber { get; set; }
    
    public VehicleDetails? VehicleDetails { get; set; }
}
