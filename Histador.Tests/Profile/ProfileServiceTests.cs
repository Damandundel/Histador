using Histador.Identity;
using Histador.Models;
using Histador.Services;
using Histador.Tests.Support;
using Microsoft.AspNetCore.Identity;

namespace Histador.Tests.Profile;

public class ProfileServiceTests
{
    private static async Task<ApplicationUser> CreateUserAsync(UserManager<ApplicationUser> users, string displayName, int hintPoints = 0)
    {
        var user = new ApplicationUser
        {
            UserName = $"{displayName}@test.com",
            Email = $"{displayName}@test.com",
            DisplayName = displayName,
            HintPoints = hintPoints,
        };
        var result = await users.CreateAsync(user, "Passw0rd!");
        Assert.True(result.Succeeded);
        return user;
    }

    [Fact]
    public async Task Own_profile_shows_hint_points()
    {
        await using var host = new IdentityTestHost();
        await IdentitySeeder.SeedAsync(host.Services);
        await using var scope = host.Scope();
        var users = IdentityTestHost.Users(scope);
        var user = await CreateUserAsync(users, "Napoleon", hintPoints: 7);

        var profile = await new ProfileService(users).BuildAsync(user, isOwnProfile: true);

        Assert.Equal(7, profile.HintPoints);
    }

    [Fact]
    public async Task Other_players_cannot_see_hint_points()
    {
        await using var host = new IdentityTestHost();
        await IdentitySeeder.SeedAsync(host.Services);
        await using var scope = host.Scope();
        var users = IdentityTestHost.Users(scope);
        var user = await CreateUserAsync(users, "Napoleon", hintPoints: 7);

        var profile = await new ProfileService(users).BuildAsync(user, isOwnProfile: false);

        Assert.Null(profile.HintPoints);
    }

    [Fact]
    public async Task Admins_get_the_admin_badge_and_players_do_not()
    {
        await using var host = new IdentityTestHost();
        await IdentitySeeder.SeedAsync(host.Services);
        await using var scope = host.Scope();
        var users = IdentityTestHost.Users(scope);
        var admin = await CreateUserAsync(users, "Herodotus");
        await users.AddToRoleAsync(admin, Roles.Admin);
        var player = await CreateUserAsync(users, "Napoleon");
        await users.AddToRoleAsync(player, Roles.Player);
        var service = new ProfileService(users);

        Assert.True((await service.BuildAsync(admin, isOwnProfile: false)).IsAdmin);
        Assert.False((await service.BuildAsync(player, isOwnProfile: false)).IsAdmin);
    }

    [Fact]
    public async Task New_player_starts_at_level_0_unranked_with_starting_mmr()
    {
        await using var host = new IdentityTestHost();
        await IdentitySeeder.SeedAsync(host.Services);
        await using var scope = host.Scope();
        var users = IdentityTestHost.Users(scope);
        var user = await CreateUserAsync(users, "Napoleon");

        var profile = await new ProfileService(users).BuildAsync(user, isOwnProfile: true);

        Assert.Equal(0, profile.Level);
        Assert.Equal(0d, profile.WorldPercent);
        Assert.Null(profile.Rank);
        Assert.Equal(ApplicationUser.StartingMmr, profile.Mmr);
        Assert.Equal("NA", profile.Initials);
    }

    [Fact]
    public async Task Display_name_lookup_ignores_case()
    {
        await using var host = new IdentityTestHost();
        await IdentitySeeder.SeedAsync(host.Services);
        await using var scope = host.Scope();
        var users = IdentityTestHost.Users(scope);
        var user = await CreateUserAsync(users, "Napoleon");
        var service = new ProfileService(users);

        var found = await service.FindByDisplayNameAsync("napoleon");
        var missing = await service.FindByDisplayNameAsync("Wellington");

        Assert.Equal(user.Id, found?.Id);
        Assert.Null(missing);
    }
}
