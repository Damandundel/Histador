using Histador.Identity;
using Histador.Tests.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Histador.Tests.Accounts;

public class IdentitySeederTests
{
    private static readonly Dictionary<string, string?> AdminConfig = new()
    {
        ["AdminSeed:Email"] = "admin@histador.test",
        ["AdminSeed:Password"] = "Adm1n-Passw0rd!",
        ["AdminSeed:DisplayName"] = "Herodotus",
    };

    [Fact]
    public async Task Creates_player_and_admin_roles()
    {
        await using var host = new IdentityTestHost();

        await IdentitySeeder.SeedAsync(host.Services);

        await using var scope = host.Scope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        Assert.True(await roles.RoleExistsAsync(Roles.Player));
        Assert.True(await roles.RoleExistsAsync(Roles.Admin));
    }

    [Fact]
    public async Task Creates_admin_from_configuration_who_can_sign_in()
    {
        await using var host = new IdentityTestHost(AdminConfig);

        await IdentitySeeder.SeedAsync(host.Services);

        await using var scope = host.Scope();
        var users = IdentityTestHost.Users(scope);
        var admin = await users.FindByEmailAsync("admin@histador.test");
        Assert.NotNull(admin);
        Assert.Equal("Herodotus", admin!.DisplayName);
        Assert.True(await users.IsInRoleAsync(admin, Roles.Admin));
        Assert.True(await users.CheckPasswordAsync(admin, "Adm1n-Passw0rd!"));
    }

    [Fact]
    public async Task Seeding_twice_does_not_duplicate_the_admin()
    {
        await using var host = new IdentityTestHost(AdminConfig);

        await IdentitySeeder.SeedAsync(host.Services);
        await IdentitySeeder.SeedAsync(host.Services);

        await using var scope = host.Scope();
        Assert.Single(IdentityTestHost.Users(scope).Users);
    }

    [Fact]
    public async Task No_admin_is_created_without_configuration()
    {
        await using var host = new IdentityTestHost();

        await IdentitySeeder.SeedAsync(host.Services);

        await using var scope = host.Scope();
        Assert.Empty(IdentityTestHost.Users(scope).Users);
    }
}
