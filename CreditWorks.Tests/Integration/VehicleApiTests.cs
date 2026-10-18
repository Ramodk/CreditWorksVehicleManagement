using System;
using System.Net;
using Xunit;

namespace CreditWorks.Tests.Integration;

[Collection("Database")]
public class VehicleApiTests
{
    [Fact]
    public async Task GetVehicle_WithUnknownId_ReturnsNotFound()
    {
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/vehicles/999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_WithInvalidWeight_ReturnsBadRequest()
    {
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/vehicle-categories/by-weight?weight=abc");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_WithNegativeWeight_ReturnsBadRequest()
    {
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/vehicle-categories/by-weight?weight=-100");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}

