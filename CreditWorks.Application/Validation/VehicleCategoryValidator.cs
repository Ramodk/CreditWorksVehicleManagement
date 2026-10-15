using System;

using CreditWorks.Domain.Entities;

namespace CreditWorks.Application.Validation;

public static class VehicleCategoryValidator
{
    public static List<string> Validate(
        IEnumerable<VehicleCategory> categories)
    {
        var errors = new List<string>();

        var ordered = categories
            .OrderBy(c => c.MinWeight)
            .ToList();

        if (ordered.Count == 0)
        {
            errors.Add("At least one category is required.");
            return errors;
        }

        // Categories must cover all positive weights.
        if (ordered[0].MinWeight != 0m)
        {
            errors.Add(
                "The first category must start at 0 kg.");
        }

        // Check individual category ranges.
        foreach (var category in ordered)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                errors.Add("Category name is required.");
            }

            if (category.MinWeight < 0m)
            {
                errors.Add(
                    "Category minimum weight cannot be negative.");
            }

            if (category.MaxWeight.HasValue &&
                category.MaxWeight.Value <= category.MinWeight)
            {
                errors.Add(
                    $"Invalid weight range for {category.Name}.");
            }
        }

        // Check gaps and overlaps.
        for (int i = 0; i < ordered.Count - 1; i++)
        {
            var current = ordered[i];
            var next = ordered[i + 1];

            if (!current.MaxWeight.HasValue)
            {
                errors.Add(
                    $"Category {current.Name} has no maximum " +
                    "but is not the last category.");
                continue;
            }

            if (current.MaxWeight.Value < next.MinWeight)
            {
                errors.Add(
                    $"Gap between {current.Name} and {next.Name}.");
            }
            else if (current.MaxWeight.Value > next.MinWeight)
            {
                errors.Add(
                    $"Overlap between {current.Name} and {next.Name}.");
            }
        }

        // Final category must have unlimited upper bound.
        if (ordered[^1].MaxWeight.HasValue)
        {
            errors.Add(
                "The last category must have no maximum weight.");
        }

        return errors;
    }

    public static VehicleCategory? FindCategory(
        decimal weight,
        IEnumerable<VehicleCategory> categories)
    {
        if (weight <= 0m)
        {
            return null;
        }

        return categories.SingleOrDefault(c =>
            weight >= c.MinWeight &&
            (!c.MaxWeight.HasValue ||
             weight < c.MaxWeight.Value));
    }
}