namespace Insurance.Service.Services;

public interface IInsuranceService
{
    Task<IEnumerable<Models.Insurance>> GetInsurancesForPinAsync(string personalIdentityNumber);
}
