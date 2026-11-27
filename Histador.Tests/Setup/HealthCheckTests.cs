using System.Net;
using Histador.Data;
using Histador.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Histador.Tests.Setup;

/// <summary>
/// /health is what Azure App Service and the deploy workflow ping after every deployment.
/// It must answer without a login.
/// </summary>
public class HealthCheckTests
{
    [Fact]
    public async Task Health_is_200_Healthy_without_login_when_the_database_is_reachable()
    {
        using var app = new HistadorAppFactory();
        var response = await app.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Health_is_503_Unhealthy_when_the_database_becomes_unreachable()
    {
        var dir = Path.Combine(Path.GetTempPath(), "histador-health-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            // Start on a real database file so startup (create + seed) succeeds...
            var connectionString = $"Data Source={Path.Combine(dir, "app.db")};Pooling=False";
            using var baseApp = new HistadorAppFactory();
            using var app = baseApp.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connectionString));
            }));
            var client = app.CreateClient();
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);

            // ...then point it at a database that cannot be opened.
            connectionString = $"Data Source={Path.Combine(dir, "missing.db")};Mode=ReadOnly;Pooling=False";
            var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch (IOException) { }
        }
    }
}
