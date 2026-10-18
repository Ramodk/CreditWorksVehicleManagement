using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CreditWorks.Application.Interfaces;


namespace CreditWorks.Api.Controllers;

[ApiController]
[Route("api/manufacturers")]
public class ManufacturersController : ControllerBase
{
    private readonly IManufacturerService _manufacturerService;

    public ManufacturersController(
        IManufacturerService manufacturerService)
    {
        _manufacturerService = manufacturerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var manufacturers =
            await _manufacturerService.GetAllAsync();

        return Ok(manufacturers);
    }
}