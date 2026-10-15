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
                MinWeight = 0m,
                MaxWeight = 500m
            },
            new()
            {
                Id = 2,
                Name = "Medium",
                MinWeight = 500m,
                MaxWeight = 2500m
            },
            new()
            {
                Id = 3,
                Name = "Heavy",
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
    public void ZeroWeight_ShouldReturnNoCategory()
    {
        var categories = GetValidCategories();

        var result = VehicleCategoryValidator.FindCategory(
            0m, categories);

        Assert.Null(result);
    }
}
