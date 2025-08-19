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

    public VehiclesController(IVehicleRepository vehicleRepository,
        IValidator<string> regValidator)
    {
        _vehicleRepository = vehicleRepository;
        _regValidator = regValidator;
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
        if (registrationNumbers.Length == 0)
        {
            return Ok(Enumerable.Empty<Contracts.Vehicle>());
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
