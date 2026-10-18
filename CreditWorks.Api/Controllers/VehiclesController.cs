using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CreditWorks.Api.Models;
using CreditWorks.Application.Interfaces;


namespace CreditWorks.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly IVehicleCategoryService _categoryService;

    public VehiclesController(
        IVehicleService vehicleService,
        IVehicleCategoryService categoryService)
    {
        _vehicleService = vehicleService;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = false)
    {
        var vehicles = await _vehicleService
            .GetAllAsync(sortBy, descending);

        var categories = await _categoryService.GetAllAsync();

        return Ok(vehicles.Select(
            v => VehicleResponse.FromEntity(v, categories)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);

        if (vehicle == null)
            return NotFound();

        var categories = await _categoryService.GetAllAsync();

        return Ok(VehicleResponse.FromEntity(vehicle, categories));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] VehicleRequest request)
    {
        var created = await _vehicleService
            .CreateAsync(request.ToEntity());

        var categories = await _categoryService.GetAllAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            VehicleResponse.FromEntity(created, categories));
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] VehicleRequest request)
    {
        var updated = await _vehicleService
            .UpdateAsync(id, request.ToEntity());

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _vehicleService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}