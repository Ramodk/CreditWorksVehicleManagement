using CreditWorks.Application.Validation;
using CreditWorks.Domain.Entities;

namespace CreditWorks.Api.Models;

// Response body for a vehicle.
// Flattens the manufacturer so the JSON has no
// Vehicle -> Manufacturer -> Vehicles cycle.
public class VehicleResponse
{
    public int Id { get; set; }

    public string Owner { get; set; } = string.Empty;

    public int ManufacturerId { get; set; }

    public string ManufacturerName { get; set; } = string.Empty;

    public int Year { get; set; }

    public decimal Weight { get; set; }

    // Derived from weight using the current category rules.
    public string? CategoryName { get; set; }

    public string? CategoryIcon { get; set; }

    public static VehicleResponse FromEntity(
        Vehicle vehicle,
        IEnumerable<VehicleCategory> categories)
    {
        var category = VehicleCategoryValidator
            .FindCategory(vehicle.Weight, categories);

        return new VehicleResponse
        {
            Id = vehicle.Id,
            Owner = vehicle.Owner,
            ManufacturerId = vehicle.ManufacturerId,
            ManufacturerName = vehicle.Manufacturer?.Name ?? string.Empty,
            Year = vehicle.Year,
            Weight = vehicle.Weight,
            CategoryName = category?.Name,
            CategoryIcon = category?.Icon
        };
    }
}
