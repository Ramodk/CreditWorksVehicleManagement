using System;


using System.Net;
using System.Net.Http.Json;
using CreditWorks.Domain.Entities;

namespace CreditWorks.Tests.Integration;

[Collection("Database")]
public class VehicleDatabaseTests
{
    [Fact]
    public async Task GetAllVehicles_ReturnsOk()
    {
        using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var response =
            await client.GetAsync("/api/vehicles");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task GetManufacturers_ReturnsSeededData()
    {
        using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/manufacturers");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var manufacturers =
            await response.Content
                .ReadFromJsonAsync<List<Manufacturer>>();

        Assert.NotNull(manufacturers);
        Assert.Contains(
            manufacturers,
            m => m.Name == "Toyota");
    }

    [Fact]
    public async Task GetCategories_ReturnsSeededData()
    {
        using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/vehicle-categories");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var categories =
            await response.Content
                .ReadFromJsonAsync<List<VehicleCategory>>();

        Assert.NotNull(categories);
        Assert.Contains(
            categories,
            c => c.Name == "Heavy");
    }
}
