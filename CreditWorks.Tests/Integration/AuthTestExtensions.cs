using System.Net.Http.Json;

namespace CreditWorks.Tests.Integration;

public static class AuthTestExtensions
{
    // The client from CreateClient() keeps cookies, so later
    // requests on the same client are authenticated.
    public static async Task LoginAsAdminAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = CustomWebApplicationFactory.AdminUsername,
            password = CustomWebApplicationFactory.AdminPassword
        });

        response.EnsureSuccessStatusCode();
    }
}
