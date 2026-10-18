using System;
using System.Net;

namespace CreditWorks.Tests.Integration;

[Collection("Database")]
public class VehicleCategoryApiTests
{
    [Fact]
    public async Task GetByWeight_NegativeWeight_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/vehicle-categories/by-weight?weight=-100");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task GetByWeight_InvalidWeight_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/vehicle-categories/by-weight?weight=invalid");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}