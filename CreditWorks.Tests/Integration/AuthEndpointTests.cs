using System.Net;
using System.Net.Http.Json;
using CreditWorks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CreditWorks.Tests.Integration;

[Collection("Database")]
public class AuthEndpointTests : IAsyncLifetime
{
    private const int Mazda = 1;

    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ThenMe_ReturnsUser()
    {
        await _client.LoginAsAdminAsync();

        var me = await _client.GetFromJsonAsync<MeResponse>("/api/auth/me");

        Assert.Equal(CustomWebApplicationFactory.AdminUsername, me!.Username);
    }

    [Theory]
    [InlineData(CustomWebApplicationFactory.AdminUsername, "wrong-password")]
    [InlineData("someone-else", CustomWebApplicationFactory.AdminPassword)]
    public async Task Login_WithWrongCredentials_ReturnsUnauthorized(
        string username, string password)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new { username, password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Login_TooManyAttempts_IsRateLimited()
    {
        HttpResponseMessage response = null!;

        for (var i = 0; i < 6; i++)
        {
            response = await _client.PostAsJsonAsync("/api/auth/login",
                new { username = "admin", password = "guess" });
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task AdminUser_IsStoredWithHashedPassword()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var admin = await context.AdminUsers.SingleAsync(
            u => u.Username == CustomWebApplicationFactory.AdminUsername);

        Assert.NotEqual(CustomWebApplicationFactory.AdminPassword, admin.PasswordHash);
        Assert.DoesNotContain(CustomWebApplicationFactory.AdminPassword, admin.PasswordHash);
    }

    [Fact]
    public async Task Login_UsernameWithSurroundingSpaces_IsAccepted()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = $"  {CustomWebApplicationFactory.AdminUsername} ",
            password = CustomWebApplicationFactory.AdminPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        await _client.LoginAsAdminAsync();

        var logout = await _client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task WriteEndpoints_WithoutLogin_ReturnUnauthorized()
    {
        var vehicle = new
        {
            owner = "Anonymous",
            manufacturerId = Mazda,
            year = 2020,
            weight = 1000m
        };

        var responses = new[]
        {
            await _client.PostAsJsonAsync("/api/vehicles", vehicle),
            await _client.PutAsJsonAsync("/api/vehicles/1", vehicle),
            await _client.DeleteAsync("/api/vehicles/1"),
            await _client.PutAsJsonAsync(
                "/api/vehicle-categories", Array.Empty<object>())
        };

        Assert.All(responses, r =>
            Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode));
    }

    [Fact]
    public async Task ReadEndpoints_WithoutLogin_StayPublic()
    {
        Assert.Equal(HttpStatusCode.OK,
            (await _client.GetAsync("/api/vehicles")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await _client.GetAsync("/api/vehicle-categories")).StatusCode);
    }

    private record MeResponse(string Username);
}
