using FluentValidation;
using Insurance.Service.Extensions;
using Insurance.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Service.Controllers;

[ApiController]
[Route("api/v1/insurances")]
public class InsurancesController : ControllerBase
{
    private readonly IInsuranceService _insuranceService;
    private readonly IValidator<string> _pinValidator;

    public InsurancesController(IInsuranceService insuranceService, IValidator<string> pinValidator)
    {
        _insuranceService = insuranceService;
        _pinValidator = pinValidator;
    }

    [HttpGet("{personalIdentityNumber}")]
    public async Task<IActionResult> GetInsurances(string personalIdentityNumber)
    {
        var validationResult = await _pinValidator.ValidateAsync(personalIdentityNumber);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var insurancesEntity = await _insuranceService.GetInsurancesForPinAsync(personalIdentityNumber);

        var insurancesContract = insurancesEntity.Select(x => x.MapToContracts());
        
        return Ok(insurancesContract);
    }
}
