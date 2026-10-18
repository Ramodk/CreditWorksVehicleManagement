using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CreditWorks.Api.Models;
using CreditWorks.Domain.Entities;

namespace CreditWorks.Tests.Integration;

[Collection("Database")]
public class CategoryEndpointTests : IAsyncLifetime
{
    private const int Mazda = 1;

    private readonly List<int> _createdVehicleIds = new();

    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private List<VehicleCategory> _original = null!;

    public async Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        await _client.LoginAsAdminAsync();
        _original = await GetCategories();
    }

    public async Task DisposeAsync()
    {
        foreach (var id in _createdVehicleIds)
        {
            await _client.DeleteAsync($"/api/vehicles/{id}");
        }

        await RestoreOriginalCategories();

        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetIcons_ReturnsAllowedIcons()
    {
        var icons = await _client.GetFromJsonAsync<List<string>>(
            "/api/vehicle-categories/icons");

        Assert.NotNull(icons);
        Assert.Contains("car", icons);
        Assert.Contains("truck-heavy", icons);
    }

    [Fact]
    public async Task Update_AddingCategory_SavesItAndRecategorisesVehicles()
    {
        var vehicle = await CreateVehicle(12000m);
        Assert.Equal("Heavy", vehicle.CategoryName);

        // Split Heavy at 10000 kg and add "Extra Heavy" above it.
        var proposed = Copy(_original);
        proposed.Single(c => c.Name == "Heavy").MaxWeight = 10000m;
        proposed.Add(new VehicleCategory
        {
            Id = 0,
            Name = "Extra Heavy",
            Icon = "forklift",
            MinWeight = 10000m,
            MaxWeight = null
        });

        var response = await _client.PutAsJsonAsync(
            "/api/vehicle-categories", proposed);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var saved = await response.Content
            .ReadFromJsonAsync<List<VehicleCategory>>();
        var added = Assert.Single(saved!, c => c.Name == "Extra Heavy");
        Assert.NotEqual(0, added.Id);

        Assert.Equal("Extra Heavy", (await GetVehicle(vehicle.Id)).CategoryName);
    }

    [Fact]
    public async Task Update_DeletingCategoryAndClosingGap_MovesVehicles()
    {
        var vehicle = await CreateVehicle(1000m);
        Assert.Equal("Medium", vehicle.CategoryName);

        // Remove Medium and let Light cover its range.
        var proposed = Copy(_original)
            .Where(c => c.Name != "Medium")
            .ToList();
        proposed.Single(c => c.Name == "Light").MaxWeight = 2500m;

        var response = await _client.PutAsJsonAsync(
            "/api/vehicle-categories", proposed);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(await GetCategories(), c => c.Name == "Medium");
        Assert.Equal("Light", (await GetVehicle(vehicle.Id)).CategoryName);
    }

    [Fact]
    public async Task Update_DeletingCategoryLeavingGap_IsRejected()
    {
        var proposed = Copy(_original)
            .Where(c => c.Name != "Medium")
            .ToList();

        var response = await _client.PutAsJsonAsync(
            "/api/vehicle-categories", proposed);

        await AssertRejectedAndUnchanged(response, "Gap");
    }

    [Fact]
    public async Task Update_WithGap_IsRejected()
    {
        var proposed = Copy(_original);
        proposed.Single(c => c.Name == "Medium").MinWeight = 600m;

        var response = await _client.PutAsJsonAsync(
            "/api/vehicle-categories", proposed);

        await AssertRejectedAndUnchanged(response, "Gap");
    }

    [Fact]
    public async Task Update_WithOverlap_IsRejected()
    {
        var proposed = Copy(_original);
        proposed.Single(c => c.Name == "Light").MaxWeight = 600m;

        var response = await _client.PutAsJsonAsync(
            "/api/vehicle-categories", proposed);

        await AssertRejectedAndUnchanged(response, "Overlap");
    }

    [Fact]
    public async Task Update_WithUnknownIcon_IsRejected()
    {
        var proposed = Copy(_original);
        proposed[0].Icon = "spaceship";

        var response = await _client.PutAsJsonAsync(
            "/api/vehicle-categories", proposed);

        await AssertRejectedAndUnchanged(response, "not a valid choice");
    }

    [Fact]
    public async Task Update_WithUnknownId_IsRejected()
    {
        var proposed = Copy(_original);
        proposed[0].Id = 999999;

        var response = await _client.PutAsJsonAsync(
            "/api/vehicle-categories", proposed);

        await AssertRejectedAndUnchanged(response, "no longer exist");
    }

    [Fact]
    public async Task Update_WithEmptyList_IsRejected()
    {
        var response = await _client.PutAsJsonAsync(
            "/api/vehicle-categories", new List<VehicleCategory>());

        await AssertRejectedAndUnchanged(response, "At least one category");
    }

    // ---------- Helpers ----------

    private async Task AssertRejectedAndUnchanged(
        HttpResponseMessage response, string expectedMessage)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Contains(
            expectedMessage,
            json.RootElement.GetProperty("detail").GetString());

        var after = await GetCategories();

        Assert.Equal(
            _original.Select(c => (c.Id, c.Name, c.MinWeight, c.MaxWeight, c.Icon)),
            after.Select(c => (c.Id, c.Name, c.MinWeight, c.MaxWeight, c.Icon)));
    }

    private async Task<List<VehicleCategory>> GetCategories()
    {
        var categories = await _client
            .GetFromJsonAsync<List<VehicleCategory>>(
                "/api/vehicle-categories");

        Assert.NotNull(categories);

        return categories;
    }

    private async Task<VehicleResponse> CreateVehicle(decimal weight)
    {
        var response = await _client.PostAsJsonAsync("/api/vehicles", new
        {
            owner = $"Test-{Guid.NewGuid():N}",
            manufacturerId = Mazda,
            year = 2020,
            weight
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var vehicle = await response.Content
            .ReadFromJsonAsync<VehicleResponse>();

        Assert.NotNull(vehicle);
        _createdVehicleIds.Add(vehicle.Id);

        return vehicle;
    }

    private async Task<VehicleResponse> GetVehicle(int id)
    {
        var vehicle = await _client.GetFromJsonAsync<VehicleResponse>(
            $"/api/vehicles/{id}");

        Assert.NotNull(vehicle);

        return vehicle;
    }

    private static List<VehicleCategory> Copy(
        IEnumerable<VehicleCategory> categories)
    {
        return categories
            .Select(c => new VehicleCategory
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon,
                MinWeight = c.MinWeight,
                MaxWeight = c.MaxWeight
            })
            .ToList();
    }

    // Puts the configuration back as it was before the test.
    // Categories the test deleted are re-added with a new Id.
    private async Task RestoreOriginalCategories()
    {
        var currentIds = (await GetCategories())
            .Select(c => c.Id)
            .ToHashSet();

        var restore = Copy(_original);

        foreach (var category in restore)
        {
            if (!currentIds.Contains(category.Id))
            {
                category.Id = 0;
            }
        }

        var response = await _client.PutAsJsonAsync(
            "/api/vehicle-categories", restore);

        response.EnsureSuccessStatusCode();
    }
}
