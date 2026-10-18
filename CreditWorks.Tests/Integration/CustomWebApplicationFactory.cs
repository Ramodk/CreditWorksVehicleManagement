using System;


using CreditWorks.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CreditWorks.Tests.Integration;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    // The default admin seeded by the AddAdminUsers migration.
    public const string AdminUsername = "admin";
    public const string AdminPassword = "Admin@123";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var connectionString =
                Environment.GetEnvironmentVariable(
                    "CREDITWORKS_TEST_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "CREDITWORKS_TEST_CONNECTION is required.");
            }

            var parsed = new Microsoft.Data.SqlClient
                .SqlConnectionStringBuilder(connectionString);

            if (!string.Equals(
                    parsed.InitialCatalog,
                    "CreditWorksVehicleManagement_Test",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Integration tests require the dedicated test database.");
            }

            services.AddDbContext<ApplicationDbContext>(
                (serviceProvider, options) =>
                {
                    options.UseSqlServer(connectionString);
                });
        });
    }
}
