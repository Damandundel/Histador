using System.Net;
using Histador.Tests.Support;

namespace Histador.Tests.Setup;

public class NavigationTests : IClassFixture<HistadorAppFactory>
{
    private readonly HttpClient _client;

    public NavigationTests(HistadorAppFactory app) => _client = app.CreateClient();

    public static readonly TheoryData<string> Sections = new() { "/Map", "/Lessons", "/Random", "/Duels", "/Profile" };
    public static readonly TheoryData<string> PlaceholderSections = new() { "/Lessons", "/Random", "/Duels", "/Profile" };

    [Theory]
    [MemberData(nameof(PlaceholderSections))]
    public async Task Remaining_main_sections_have_a_placeholder_page(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Coming soon", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Map_page_renders_world_map()
    {
        var response = await _client.GetAsync("/Map");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("World Map", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Top_nav_links_to_every_main_section()
    {
        var html = await _client.GetStringAsync("/");

        foreach (var url in Sections)
            Assert.Contains($"href=\"{url}\"", html);
    }
}
