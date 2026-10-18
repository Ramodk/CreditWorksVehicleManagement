using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CreditWorks.Application.Interfaces;
using CreditWorks.Api.Models;
using CreditWorks.Application.Validation;


namespace CreditWorks.Api.Controllers;

[ApiController]
[Route("api/vehicle-categories")]
public class VehicleCategoriesController : ControllerBase
{
    private readonly IVehicleCategoryService _categoryService;

    public VehicleCategoriesController(
        IVehicleCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("by-weight")]
    public async Task<IActionResult> GetByWeight(
        [FromQuery] decimal weight)
    {
        if (weight < 0)
            return BadRequest("Weight cannot be negative.");

        var category = await _categoryService
            .GetByWeightAsync(weight);

        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpGet("icons")]
    public IActionResult GetIcons()
    {
        return Ok(CategoryIcons.Allowed);
    }

    // Replaces the whole configuration so that related edits
    // (e.g. moving a boundary between two categories) are
    // validated and saved together.
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateCategories(
        [FromBody] List<CategoryRequest> categories)
    {
        var saved = await _categoryService.UpdateCategoriesAsync(
            categories.Select(c => c.ToEntity()).ToList());

        return Ok(saved);
    }
} 