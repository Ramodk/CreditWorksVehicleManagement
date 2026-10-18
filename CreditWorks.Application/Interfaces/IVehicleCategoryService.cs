using System;

using CreditWorks.Domain.Entities;

namespace CreditWorks.Application.Interfaces;

public interface IVehicleCategoryService
{
    Task<List<VehicleCategory>> GetAllAsync();

    Task<VehicleCategory?> GetByWeightAsync(decimal weight);

    // Replaces the whole category configuration in one save:
    // Id 0 adds, a known Id updates, and a missing Id deletes.
    // Throws ValidationException if the result would be invalid.
    Task<List<VehicleCategory>> UpdateCategoriesAsync(
        List<VehicleCategory> categories);
}