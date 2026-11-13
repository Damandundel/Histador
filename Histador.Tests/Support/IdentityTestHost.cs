using Histador.Data;
using Histador.Identity;
using Histador.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Histador.Tests.Support;

/// <summary>
/// A real Identity stack (same registration as the app) on an in-memory SQLite database.
/// </summary>
public sealed class IdentityTestHost : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public ServiceProvider Services { get; }

    public IdentityTestHost(IDictionary<string, string?>? config = null)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(config ?? new Dictionary<string, string?>())
            .Build());
        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));
        services.AddHistadorIdentity();
        Services = services.BuildServiceProvider();

        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    public AsyncServiceScope Scope() => Services.CreateAsyncScope();

    public static UserManager<ApplicationUser> Users(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    public async ValueTask DisposeAsync()
    {
        await Services.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
