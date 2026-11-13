using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Histador.Data;
using Histador.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Histador.Tests.Support;

/// <summary>
/// Runs the real app in memory with SQLite instead of SQL Server, a seeded admin,
/// a captured email outbox, and an optional header-based test login.
/// </summary>
public sealed class HistadorAppFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@histador.test";
    public const string AdminPassword = "Adm1n-Passw0rd!";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public CapturingEmailSender Outbox { get; } = new();

    public HistadorAppFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Initialize"] = "EnsureCreated",
            ["AdminSeed:Email"] = AdminEmail,
            ["AdminSeed:Password"] = AdminPassword,
            ["AdminSeed:DisplayName"] = "Herodotus",
        }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));

            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Outbox);

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { })
                .AddPolicyScheme("TestOrCookie", null, o => o.ForwardDefaultSelector = ctx =>
                    ctx.Request.Headers.ContainsKey(TestAuthHandler.UserHeader)
                        ? TestAuthHandler.SchemeName
                        : IdentityConstants.ApplicationScheme);
            services.PostConfigure<AuthenticationOptions>(o => o.DefaultAuthenticateScheme = "TestOrCookie");
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}

public sealed class CapturingEmailSender : IEmailSender
{
    public ConcurrentQueue<(string To, string Subject, string HtmlBody)> Sent { get; } = new();

    public Task SendAsync(string to, string subject, string htmlBody)
    {
        Sent.Enqueue((to, subject, htmlBody));
        return Task.CompletedTask;
    }
}

/// <summary>Signs the request in as whoever the X-Test-User / X-Test-Roles headers say.</summary>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string UserHeader = "X-Test-User";
    public const string RolesHeader = "X-Test-Roles";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeader, out var user))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim> { new(ClaimTypes.Name, user.ToString()), new(ClaimTypes.NameIdentifier, user.ToString()) };
        foreach (var role in Request.Headers[RolesHeader].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries))
            claims.Add(new Claim(ClaimTypes.Role, role.Trim()));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
