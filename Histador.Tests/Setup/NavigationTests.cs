using System.Net;
using Histador.Tests.Support;

namespace Histador.Tests.Setup;

public class NavigationTests : IClassFixture<HistadorAppFactory>
{
    private readonly HttpClient _client;

    public NavigationTests(HistadorAppFactory app)
    {
        _client = app.CreateClient();
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "player-1");
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, "Player");
    }

    public static readonly TheoryData<string> Sections = new() { "/Map", "/Lessons", "/Random", "/Duels", "/Profile" };

    [Theory]
    [MemberData(nameof(Sections))]
    public async Task Each_main_section_has_a_placeholder_page(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Coming soon", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Top_nav_links_to_every_main_section()
    {
        var html = await _client.GetStringAsync("/");

        foreach (var url in Sections)
            Assert.Contains($"href=\"{url}\"", html);
    }
}
