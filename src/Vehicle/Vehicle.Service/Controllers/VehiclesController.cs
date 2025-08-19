using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Vehicle.Service.Extensions;
using Vehicle.Service.Repositories;

namespace Vehicle.Service.Controllers;

[ApiController]
[Route("api/v1/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IValidator<string> _regValidator;
    private readonly IValidator<string[]> _regListValidator;

    public VehiclesController(IVehicleRepository vehicleRepository,
        IValidator<string> regValidator, IValidator<string[]> regListValidator)
    {
        _vehicleRepository = vehicleRepository;
        _regValidator = regValidator;
        _regListValidator = regListValidator;
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
        var validation = await _regListValidator.ValidateAsync(registrationNumbers);
        
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors);
        }

        var vehicleEntities = await _vehicleRepository.GetVehiclesByRegistrationNumbersAsync(registrationNumbers);

        if (vehicleEntities.Length == 0)
        {
            return NotFound();
        }

        // Map from the data models to the contract models
        var vehicleContracts = vehicleEntities.Select(x => x.MapToContracts());
        
        return Ok(vehicleContracts);
    }
}
