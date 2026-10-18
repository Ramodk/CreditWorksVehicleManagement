using CreditWorks.Domain.Entities;

namespace CreditWorks.Api.Models;

// One category in a full category configuration.
// Id 0 means a new category.
public class CategoryRequest
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal MinWeight { get; set; }

    // Null means no upper limit (the last category).
    public decimal? MaxWeight { get; set; }

    public string Icon { get; set; } = string.Empty;

    public VehicleCategory ToEntity()
    {
        return new VehicleCategory
        {
            Id = Id,
            Name = Name,
            MinWeight = MinWeight,
            MaxWeight = MaxWeight,
            Icon = Icon
        };
    }
}
