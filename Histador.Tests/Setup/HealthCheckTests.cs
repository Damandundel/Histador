using System.Net;
using Histador.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Histador.Tests.Setup;

/// <summary>
/// /health is what Azure App Service and the deploy workflow ping after every deployment.
/// </summary>
public class HealthCheckTests
{
    [Fact]
    public async Task Health_is_200_Healthy_when_the_database_is_reachable()
    {
        using var app = new AppWithDatabase("Data Source=:memory:");
        var response = await app.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Health_is_503_Unhealthy_when_the_database_is_unreachable()
    {
        var missingFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.db");
        using var app = new AppWithDatabase($"Data Source={missingFile};Mode=ReadOnly");
        var response = await app.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
    }

    /// <summary>The real app, with SQL Server swapped for a SQLite connection string.</summary>
    private sealed class AppWithDatabase(string sqliteConnectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(sqliteConnectionString));
            });
        }
    }
}
