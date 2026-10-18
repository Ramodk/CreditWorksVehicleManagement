using CreditWorks.Domain.Entities;

namespace CreditWorks.Api.Models;

// Request body for creating/updating a vehicle.
// Only carries the fields the client is allowed to set,
// so navigation properties like Manufacturer are not required.
public class VehicleRequest
{
    public string Owner { get; set; } = string.Empty;

    public int ManufacturerId { get; set; }

    public int Year { get; set; }

    public decimal Weight { get; set; }

    public Vehicle ToEntity()
    {
        return new Vehicle
        {
            Owner = Owner,
            ManufacturerId = ManufacturerId,
            Year = Year,
            Weight = Weight
        };
    }
}
