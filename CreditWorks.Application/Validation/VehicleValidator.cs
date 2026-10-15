using System;

using CreditWorks.Domain.Entities;

namespace CreditWorks.Application.Validation;

public static class VehicleValidator
{
    public static List<string> Validate(Vehicle vehicle)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(vehicle.Owner))
        {
            errors.Add("Vehicle owner is required.");
        }
        else if (vehicle.Owner.Length > 150)
        {
            errors.Add("Owner name cannot exceed 150 characters.");
        }

        if (vehicle.ManufacturerId <= 0)
        {
            errors.Add("A manufacturer must be selected.");
        }

        if (vehicle.Year < 1886 ||
            vehicle.Year > DateTime.UtcNow.Year + 1)
        {
            errors.Add("Manufacturing year is invalid.");
        }

        if (vehicle.Weight <= 0)
        {
            errors.Add("Vehicle weight must be greater than zero.");
        }

        if (decimal.Round(vehicle.Weight, 2) != vehicle.Weight)
        {
            errors.Add("Vehicle weight cannot have more than two decimal places.");
        }

        return errors;
    }
}