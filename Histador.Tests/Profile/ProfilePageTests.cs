using System.Net;
using Histador.Models;
using Histador.Tests.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Histador.Tests.Profile;

public class ProfilePageTests : IClassFixture<HistadorAppFactory>
{
    private readonly HistadorAppFactory _app;

    public ProfilePageTests(HistadorAppFactory app) => _app = app;

    private async Task<ApplicationUser> CreateUserAsync(string displayName, int hintPoints = 0)
    {
        using var scope = _app.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
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

    private HttpClient ClientFor(ApplicationUser user)
    {
        var client = _app.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, user.Id);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, "Player");
        return client;
    }

    private static string UniqueName(string prefix) => prefix + Guid.NewGuid().ToString("N")[..8];

    [Fact]
    public async Task Own_profile_shows_name_and_hint_points()
    {
        var me = await CreateUserAsync(UniqueName("Me"), hintPoints: 5);

        var response = await ClientFor(me).GetAsync("/Profile");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(me.DisplayName, html);
        Assert.Contains("Hint points", html);
        Assert.DoesNotContain("Challenge to duel", html);
    }

    [Fact]
    public async Task Other_players_profile_hides_hint_points_and_shows_social_buttons()
    {
        var me = await CreateUserAsync(UniqueName("Me"));
        var other = await CreateUserAsync(UniqueName("Other"), hintPoints: 9);

        var response = await ClientFor(me).GetAsync($"/Profile/{other.DisplayName}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(other.DisplayName, html);
        Assert.DoesNotContain("Hint points", html);
        Assert.Contains("Add friend", html);
        Assert.Contains("Challenge to duel", html);
    }

    [Fact]
    public async Task Unknown_display_name_returns_404()
    {
        var me = await CreateUserAsync(UniqueName("Me"));

        var response = await ClientFor(me).GetAsync("/Profile/NobodyByThisName");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Seeded_admin_profile_shows_the_admin_badge()
    {
        var me = await CreateUserAsync(UniqueName("Me"));

        var html = await ClientFor(me).GetStringAsync("/Profile/Herodotus");

        Assert.Contains("badge", html);
        Assert.Contains(">Admin</span>", html);
    }
}
