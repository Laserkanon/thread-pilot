using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Vehicle.Service.Extensions;
using Vehicle.Service.Repositories;
using Vehicle.Service.Services;

namespace Vehicle.Service.Controllers;

[ApiController]
[Route("api/v1/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IValidator<string> _regValidator;
    private readonly IRegistrationNumberValidatorService _registrationNumberValidatorService;

    public VehiclesController(IVehicleRepository vehicleRepository,
        IValidator<string> regValidator,
        IRegistrationNumberValidatorService registrationNumberValidatorService)
    {
        _vehicleRepository = vehicleRepository;
        _regValidator = regValidator;
        _registrationNumberValidatorService = registrationNumberValidatorService;
    }

    [HttpGet("{registrationNumber}")]
    public async Task<IActionResult> GetVehicle(string registrationNumber)
    {
        var validation = await _regValidator.ValidateAsync(registrationNumber);
        
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors);
        }

        var vehicleEntity = await _vehicleRepository.GetVehicleByRegistrationNumberAsync(registrationNumber);

        if (vehicleEntity == null)
        {
            return NotFound();
        }

        // Map from the data model to the contract model
        var vehicleContract = vehicleEntity.MapToContracts();

        return Ok(vehicleContract);
    }

    [HttpPost("batch")]
    public async Task<IActionResult> GetVehiclesBatch([FromBody] string[] registrationNumbers)
    {
        var validRegistrationNumbers = _registrationNumberValidatorService.Validate(registrationNumbers);
        
        var vehicleEntities = await _vehicleRepository.GetVehiclesByRegistrationNumbersAsync(validRegistrationNumbers);

        // Map from the data models to the contract models
        var vehicleContracts = vehicleEntities.Select(x => x.MapToContracts());
        
        return Ok(vehicleContracts);
    }
}
