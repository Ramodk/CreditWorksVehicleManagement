using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CreditWorks.Api.Models;
using CreditWorks.Domain.Entities;

namespace CreditWorks.Tests.Integration;

[Collection("Database")]
public class VehicleEndpointTests : IAsyncLifetime
{
    // Seeded manufacturer IDs.
    private const int Mazda = 1;
    private const int Honda = 3;
    private const int Toyota = 5;

    // Unique per test so we only look at rows this test created.
    private readonly string _ownerPrefix =
        $"Test-{Guid.NewGuid():N}";

    private readonly List<int> _createdIds = new();

    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        await _client.LoginAsAdminAsync();
    }

    public async Task DisposeAsync()
    {
        foreach (var id in _createdIds)
        {
            await _client.DeleteAsync($"/api/vehicles/{id}");
        }

        _client.Dispose();
        await _factory.DisposeAsync();
    }

    // ---------- Create ----------

    [Fact]
    public async Task Create_ValidVehicle_ReturnsCreatedWithCategory()
    {
        var response = await PostVehicle(
            $"{_ownerPrefix}-Valid", Mazda, 2020, 1850.75m);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var vehicle = await ReadVehicle(response);

        Assert.Equal("Mazda", vehicle.ManufacturerName);
        Assert.Equal(1850.75m, vehicle.Weight);
        Assert.Equal("Medium", vehicle.CategoryName);
    }

    [Theory]
    [InlineData("", Mazda, 2020, 1000, "owner")]
    [InlineData("Owner", 999999, 2020, 1000, "manufacturer")]
    [InlineData("Owner", Mazda, 1800, 1000, "Year of manufacture")]
    [InlineData("Owner", Mazda, 2020, 0, "greater than zero")]
    [InlineData("Owner", Mazda, 2020, -5, "greater than zero")]
    [InlineData("Owner", Mazda, 2020, 1200.123, "two decimal places")]
    public async Task Create_InvalidVehicle_ReturnsBadRequestWithMessage(
        string owner,
        int manufacturerId,
        int year,
        decimal weight,
        string expectedMessage)
    {
        var response = await PostVehicle(
            owner, manufacturerId, year, weight);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            expectedMessage,
            await ReadProblemDetail(response),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_SeveralInvalidFields_ReturnsEachError()
    {
        var response = await PostVehicle("", Mazda, 1800, -5m);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var errors = json.RootElement.GetProperty("errors")
            .EnumerateArray()
            .Select(e => e.GetString())
            .ToList();

        Assert.Equal(3, errors.Count);
    }

    [Fact]
    public async Task Create_YearAfterNextYear_ReturnsBadRequest()
    {
        var response = await PostVehicle(
            $"{_ownerPrefix}-Future", Mazda,
            DateTime.UtcNow.Year + 2, 1000m);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyBody_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/vehicles", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- Sorting ----------

    [Theory]
    [InlineData("owner", false, "A,B,C")]
    [InlineData("owner", true, "C,B,A")]
    [InlineData("manufacturer", false, "C,B,A")]
    [InlineData("manufacturer", true, "A,B,C")]
    [InlineData("year", false, "C,A,B")]
    [InlineData("year", true, "B,A,C")]
    [InlineData("weight", false, "B,A,C")]
    [InlineData("weight", true, "C,A,B")]
    public async Task GetAll_SortsByRequestedField(
        string sortBy, bool descending, string expectedOrder)
    {
        // A: Toyota, 2015, 1200.00 kg
        // B: Mazda,  2021,  300.50 kg
        // C: Honda,  2010, 3000.25 kg
        await CreateVehicle($"{_ownerPrefix}-A", Toyota, 2015, 1200.00m);
        await CreateVehicle($"{_ownerPrefix}-B", Mazda, 2021, 300.50m);
        await CreateVehicle($"{_ownerPrefix}-C", Honda, 2010, 3000.25m);

        var vehicles = await GetOwnVehicles(
            $"?sortBy={sortBy}&descending={descending}");

        var actualOrder = string.Join(
            ",", vehicles.Select(v => v.Owner[^1..]));

        Assert.Equal(expectedOrder, actualOrder);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetAll_EqualSortValues_AreOrderedById(bool descending)
    {
        var owner = $"{_ownerPrefix}-Same";

        var first = await CreateVehicle(owner, Mazda, 2020, 1000m);
        var second = await CreateVehicle(owner, Honda, 2018, 2000m);
        var third = await CreateVehicle(owner, Toyota, 2022, 3000m);

        var vehicles = await GetOwnVehicles(
            $"?sortBy=owner&descending={descending}");

        Assert.Equal(
            new[] { first.Id, second.Id, third.Id },
            vehicles.Select(v => v.Id));
    }

    // ---------- Category changes ----------

    [Fact]
    public async Task GetById_AfterCategoryRangesChange_ShowsNewCategory()
    {
        var vehicle = await CreateVehicle(
            $"{_ownerPrefix}-Recategorise", Mazda, 2020, 2200m);

        Assert.Equal("Medium", vehicle.CategoryName);

        var original = await _client.GetFromJsonAsync<List<VehicleCategory>>(
            "/api/vehicle-categories");
        Assert.NotNull(original);

        try
        {
            // Move the Medium/Heavy boundary from 2500 kg to 2000 kg.
            var changed = original
                .Select(c => new VehicleCategory
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    MinWeight = c.Name == "Heavy" ? 2000m : c.MinWeight,
                    MaxWeight = c.Name == "Medium" ? 2000m : c.MaxWeight
                })
                .ToList();

            var update = await _client.PutAsJsonAsync(
                "/api/vehicle-categories", changed);
            Assert.Equal(HttpStatusCode.OK, update.StatusCode);

            var reloaded = await _client.GetFromJsonAsync<VehicleResponse>(
                $"/api/vehicles/{vehicle.Id}");

            Assert.Equal("Heavy", reloaded?.CategoryName);
        }
        finally
        {
            await _client.PutAsJsonAsync(
                "/api/vehicle-categories", original);
        }
    }

    // ---------- Helpers ----------

    private Task<HttpResponseMessage> PostVehicle(
        string owner, int manufacturerId, int year, decimal weight)
    {
        return _client.PostAsJsonAsync("/api/vehicles", new
        {
            owner,
            manufacturerId,
            year,
            weight
        });
    }

    private async Task<VehicleResponse> CreateVehicle(
        string owner, int manufacturerId, int year, decimal weight)
    {
        var response = await PostVehicle(
            owner, manufacturerId, year, weight);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await ReadVehicle(response);
    }

    private async Task<VehicleResponse> ReadVehicle(
        HttpResponseMessage response)
    {
        var vehicle = await response.Content
            .ReadFromJsonAsync<VehicleResponse>();

        Assert.NotNull(vehicle);
        _createdIds.Add(vehicle.Id);

        return vehicle;
    }

    private async Task<List<VehicleResponse>> GetOwnVehicles(string query)
    {
        var vehicles = await _client.GetFromJsonAsync<List<VehicleResponse>>(
            $"/api/vehicles{query}");

        Assert.NotNull(vehicles);

        return vehicles
            .Where(v => v.Owner.StartsWith(_ownerPrefix))
            .ToList();
    }

    private static async Task<string> ReadProblemDetail(
        HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        return json.RootElement.TryGetProperty("detail", out var detail)
            ? detail.GetString() ?? string.Empty
            : string.Empty;
    }
}
