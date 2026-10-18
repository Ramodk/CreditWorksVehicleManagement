using System;

using CreditWorks.Application.Validation;
using CreditWorks.Domain.Entities;
using Xunit;

namespace CreditWorks.Tests;

public class VehicleValidatorTests
{
    private static Vehicle GetValidVehicle()
    {
        return new Vehicle
        {
            Owner = "John Smith",
            ManufacturerId = 1,
            Year = 2022,
            Weight = 1450.50m
        };
    }

    [Fact]
    public void ValidVehicle_ShouldHaveNoErrors()
    {
        var vehicle = GetValidVehicle();

        var errors = VehicleValidator.Validate(vehicle);

        Assert.Empty(errors);
    }

    [Fact]
    public void EmptyOwner_ShouldBeRejected()
    {
        var vehicle = GetValidVehicle();
        vehicle.Owner = "";

        var errors = VehicleValidator.Validate(vehicle);

        Assert.Contains(errors,
            e => e.Contains("owner is required"));
    }

    [Fact]
    public void NegativeWeight_ShouldBeRejected()
    {
        var vehicle = GetValidVehicle();
        vehicle.Weight = -100m;

        var errors = VehicleValidator.Validate(vehicle);

        Assert.Contains(errors,
            e => e.Contains("greater than zero"));
    }

    [Fact]
    public void WeightWithThreeDecimalPlaces_ShouldBeRejected()
    {
        var vehicle = GetValidVehicle();
        vehicle.Weight = 1200.123m;

        var errors = VehicleValidator.Validate(vehicle);

        Assert.Contains(errors,
            e => e.Contains("two decimal places"));
    }

    [Fact]
    public void InvalidManufacturer_ShouldBeRejected()
    {
        var vehicle = GetValidVehicle();
        vehicle.ManufacturerId = 0;

        var errors = VehicleValidator.Validate(vehicle);

        Assert.Contains(errors,
            e => e.Contains("manufacturer"));
    }

    [Fact]
    public void InvalidYear_ShouldBeRejected()
    {
        var vehicle = GetValidVehicle();
        vehicle.Year = 1800;

        var errors = VehicleValidator.Validate(vehicle);

        Assert.Contains(errors,
            e => e.Contains("Year of manufacture"));
    }

    [Fact]
    public void EarliestYear_ShouldBeAccepted()
    {
        var vehicle = GetValidVehicle();
        vehicle.Year = VehicleValidator.EarliestYear;

        var errors = VehicleValidator.Validate(vehicle);

        Assert.Empty(errors);
    }

    [Fact]
    public void NextYearModel_ShouldBeAccepted()
    {
        var vehicle = GetValidVehicle();
        vehicle.Year = DateTime.UtcNow.Year + 1;

        var errors = VehicleValidator.Validate(vehicle);

        Assert.Empty(errors);
    }

    [Fact]
    public void YearAfterNextYear_ShouldBeRejected()
    {
        var vehicle = GetValidVehicle();
        vehicle.Year = DateTime.UtcNow.Year + 2;

        var errors = VehicleValidator.Validate(vehicle);

        Assert.Contains(errors,
            e => e.Contains("Year of manufacture"));
    }

    [Fact]
    public void WeightWithTwoDecimalPlaces_ShouldBeAccepted()
    {
        var vehicle = GetValidVehicle();
        vehicle.Weight = 1850.75m;

        var errors = VehicleValidator.Validate(vehicle);

        Assert.Empty(errors);
    }
}
