using System;

using CreditWorks.Application.Validation;
using CreditWorks.Domain.Entities;
using Xunit;

namespace CreditWorks.Tests;

public class VehicleCategoryValidatorTests
{
    private static List<VehicleCategory> GetValidCategories()
    {
        return new List<VehicleCategory>
        {
            new()
            {
                Id = 1,
                Name = "Light",
                Icon = "car",
                MinWeight = 0m,
                MaxWeight = 500m
            },
            new()
            {
                Id = 2,
                Name = "Medium",
                Icon = "truck",
                MinWeight = 500m,
                MaxWeight = 2500m
            },
            new()
            {
                Id = 3,
                Name = "Heavy",
                Icon = "truck-heavy",
                MinWeight = 2500m,
                MaxWeight = null
            }
        };
    }

    [Fact]
    public void ValidCategories_ShouldHaveNoErrors()
    {
        var categories = GetValidCategories();

        var errors = VehicleCategoryValidator.Validate(categories);

        Assert.Empty(errors);
    }

    [Fact]
    public void GapBetweenCategories_ShouldBeRejected()
    {
        var categories = GetValidCategories();
        categories[1].MinWeight = 600m;

        var errors = VehicleCategoryValidator.Validate(categories);

        Assert.Contains(errors, e => e.Contains("Gap"));
    }

    [Fact]
    public void OverlappingCategories_ShouldBeRejected()
    {
        var categories = GetValidCategories();
        categories[1].MinWeight = 400m;

        var errors = VehicleCategoryValidator.Validate(categories);

        Assert.Contains(errors, e => e.Contains("Overlap"));
    }

    [Fact]
    public void FinalCategoryWithMaximum_ShouldBeRejected()
    {
        var categories = GetValidCategories();
        categories[2].MaxWeight = 5000m;

        var errors = VehicleCategoryValidator.Validate(categories);

        Assert.Contains(errors,
            e => e.Contains("last category"));
    }

    [Theory]
    [InlineData(499.99, "Light")]
    [InlineData(500, "Medium")]
    [InlineData(2499.99, "Medium")]
    [InlineData(2500, "Heavy")]
    [InlineData(5000, "Heavy")]
    public void FindCategory_ShouldReturnCorrectCategory(
        decimal weight, string expectedName)
    {
        var categories = GetValidCategories();

        var result = VehicleCategoryValidator.FindCategory(
            weight, categories);

        Assert.NotNull(result);
        Assert.Equal(expectedName, result.Name);
    }

    [Fact]
    public void ChangingCategoryRanges_ShouldRecategoriseVehicle()
    {
        // Example from the assignment: a 2200 kg vehicle is Medium,
        // then becomes Heavy when Medium/Heavy move to 2000 kg.
        var categories = GetValidCategories();

        var before = VehicleCategoryValidator.FindCategory(
            2200m, categories);

        categories[1].MaxWeight = 2000m;
        categories[2].MinWeight = 2000m;

        var after = VehicleCategoryValidator.FindCategory(
            2200m, categories);

        Assert.Equal("Medium", before?.Name);
        Assert.Equal("Heavy", after?.Name);
    }

    [Fact]
    public void MissingIcon_ShouldBeRejected()
    {
        var categories = GetValidCategories();
        categories[0].Icon = "";

        var errors = VehicleCategoryValidator.Validate(categories);

        Assert.Contains(errors, e => e.Contains("icon is required"));
    }

    [Fact]
    public void UnknownIcon_ShouldBeRejected()
    {
        var categories = GetValidCategories();
        categories[0].Icon = "spaceship";

        var errors = VehicleCategoryValidator.Validate(categories);

        Assert.Contains(errors, e => e.Contains("not a valid choice"));
    }

    [Fact]
    public void DuplicateNames_ShouldBeRejected()
    {
        var categories = GetValidCategories();
        categories[1].Name = "light";

        var errors = VehicleCategoryValidator.Validate(categories);

        Assert.Contains(errors, e => e.Contains("more than once"));
    }

    [Fact]
    public void BoundaryWithThreeDecimals_ShouldBeRejected()
    {
        var categories = GetValidCategories();
        categories[0].MaxWeight = 500.001m;
        categories[1].MinWeight = 500.001m;

        var errors = VehicleCategoryValidator.Validate(categories);

        Assert.Contains(errors, e => e.Contains("two decimal places"));
    }

    [Fact]
    public void RemovingMiddleCategory_WithoutClosingGap_ShouldBeRejected()
    {
        var categories = GetValidCategories();
        categories.RemoveAt(1);

        var errors = VehicleCategoryValidator.Validate(categories);

        Assert.Contains(errors, e => e.Contains("Gap"));
    }

    [Fact]
    public void ZeroWeight_ShouldReturnNoCategory()
    {
        var categories = GetValidCategories();

        var result = VehicleCategoryValidator.FindCategory(
            0m, categories);

        Assert.Null(result);
    }
}
