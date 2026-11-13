using Histador.Identity;
using Histador.Services;
using Histador.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace Histador.Tests.Accounts;

public class AccountServiceTests
{
    [Fact]
    public async Task Registered_user_gets_the_player_role_and_chosen_language()
    {
        await using var host = new IdentityTestHost();
        await IdentitySeeder.SeedAsync(host.Services);
        await using var scope = host.Scope();
        var accounts = scope.ServiceProvider.GetRequiredService<AccountService>();

        var result = await accounts.RegisterAsync("new@test.com", "Napoleon", "Passw0rd!", "bg");

        Assert.True(result.Succeeded);
        var users = IdentityTestHost.Users(scope);
        var user = await users.FindByEmailAsync("new@test.com");
        Assert.NotNull(user);
        Assert.Equal("Napoleon", user!.DisplayName);
        Assert.Equal("bg", user.PreferredLanguage);
        Assert.True(await users.IsInRoleAsync(user, Roles.Player));
        Assert.False(await users.IsInRoleAsync(user, Roles.Admin));
    }

    [Fact]
    public async Task Unknown_language_falls_back_to_english()
    {
        await using var host = new IdentityTestHost();
        await IdentitySeeder.SeedAsync(host.Services);
        await using var scope = host.Scope();
        var accounts = scope.ServiceProvider.GetRequiredService<AccountService>();

        await accounts.RegisterAsync("new@test.com", "Napoleon", "Passw0rd!", "fr");

        var user = await IdentityTestHost.Users(scope).FindByEmailAsync("new@test.com");
        Assert.Equal("en", user!.PreferredLanguage);
    }
}
