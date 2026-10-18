using System;


using CreditWorks.Application.Exceptions;
using CreditWorks.Application.Interfaces;
using CreditWorks.Application.Validation;
using CreditWorks.Domain.Entities;
using CreditWorks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace CreditWorks.Infrastructure.Services;

public class VehicleCategoryService : IVehicleCategoryService
{
    private readonly ApplicationDbContext _context;

    public VehicleCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VehicleCategory>> GetAllAsync()
    {
        return await _context.VehicleCategories
            .AsNoTracking()
            .OrderBy(c => c.MinWeight)
            .ToListAsync();
    }

    public async Task<VehicleCategory?> GetByWeightAsync(
        decimal weight)
    {
        if (weight <= 0m)
        {
            return null;
        }

        var categories = await GetAllAsync();

        return VehicleCategoryValidator.FindCategory(
            weight, categories);
    }

    public async Task<List<VehicleCategory>> UpdateCategoriesAsync(
        List<VehicleCategory> categories)
    {
        if (categories == null || categories.Count == 0)
        {
            throw new ValidationException(
                "At least one category is required.");
        }

        // The submitted list is the complete new configuration,
        // so validating it covers adds, edits and deletes at once.
        var errors = VehicleCategoryValidator.Validate(categories);

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        var existingCategories = await _context.VehicleCategories
            .ToListAsync();

        var submittedIds = categories
            .Where(c => c.Id != 0)
            .Select(c => c.Id)
            .ToList();

        if (submittedIds.Distinct().Count() != submittedIds.Count)
        {
            throw new ValidationException(
                "Each category can only appear once.");
        }

        // An unknown Id usually means someone else deleted the
        // category since this list was loaded.
        if (submittedIds.Any(id =>
                existingCategories.All(e => e.Id != id)))
        {
            throw new ValidationException(
                "One or more categories no longer exist. " +
                "Reload the page and try again.");
        }

        _context.VehicleCategories.RemoveRange(
            existingCategories.Where(e => !submittedIds.Contains(e.Id)));

        foreach (var category in categories)
        {
            var target = category.Id == 0
                ? _context.VehicleCategories.Add(new VehicleCategory()).Entity
                : existingCategories.Single(e => e.Id == category.Id);

            target.Name = category.Name.Trim();
            target.MinWeight = category.MinWeight;
            target.MaxWeight = category.MaxWeight;
            target.Icon = category.Icon.Trim();
        }

        // One SaveChanges call runs in a single transaction, so the
        // database never holds a half-applied configuration.
        await _context.SaveChangesAsync();

        return await GetAllAsync();
    }
}
