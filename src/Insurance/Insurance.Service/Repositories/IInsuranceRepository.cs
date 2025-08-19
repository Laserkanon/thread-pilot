namespace Insurance.Service.Repositories;

public interface IInsuranceRepository
{
    Task<Models.Insurance[]> GetInsurancesByPinAsync(string personalIdentityNumber);
}
