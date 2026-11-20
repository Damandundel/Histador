using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using Histador.Models;
using Histador.Tests.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Histador.Tests.Polish;

/// <summary>
/// Accessibility basics checked on every page we have. New pages should be added to the lists below.
/// </summary>
public class AccessibilityTests : IClassFixture<HistadorAppFactory>
{
    private readonly HistadorAppFactory _app;

    public AccessibilityTests(HistadorAppFactory app) => _app = app;

    public static readonly TheoryData<string> PublicPages = new()
    {
        "/", "/Home/Privacy", "/Account/Login", "/Account/Register", "/Account/ForgotPassword",
    };

    public static readonly TheoryData<string> PlayerPages = new()
    {
        "/Map", "/Profile", "/Lessons", "/Random", "/Duels", "/Account/ChangePassword",
    };

    public static readonly TheoryData<string> AllPages = new()
    {
        "/", "/Home/Privacy", "/Account/Login", "/Account/Register", "/Account/ForgotPassword",
        "/Map", "/Profile", "/Lessons", "/Random", "/Duels", "/Account/ChangePassword", "/Admin",
    };

    private string? _playerId;

    private async Task<HttpClient> ClientForAsync(string url)
    {
        var client = _app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        if (((IEnumerable<object[]>)PublicPages).Any(p => (string)p[0] == url))
            return client;

        _playerId ??= await CreatePlayerAsync();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, _playerId);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, url == "/Admin" ? "Player,Admin" : "Player");
        return client;
    }

    private async Task<string> CreatePlayerAsync()
    {
        using var scope = _app.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var name = "A11y" + Guid.NewGuid().ToString("N")[..8];
        var user = new ApplicationUser { UserName = $"{name}@test.com", Email = $"{name}@test.com", DisplayName = name };
        Assert.True((await users.CreateAsync(user, "Passw0rd!")).Succeeded);
        return user.Id;
    }

    private async Task<IDocument> LoadAsync(string url)
    {
        var client = await ClientForAsync(url);
        var response = await client.GetAsync(url);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{url} returned {(int)response.StatusCode}");
        var html = await response.Content.ReadAsStringAsync();
        return await BrowsingContext.New(Configuration.Default).OpenAsync(req => req.Content(html));
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task Page_has_a_language_a_meaningful_title_and_one_h1(string url)
    {
        var doc = await LoadAsync(url);

        Assert.False(string.IsNullOrWhiteSpace(doc.DocumentElement.GetAttribute("lang")), "html needs lang");
        var title = doc.Title?.Replace("- Histador", "").Trim();
        Assert.False(string.IsNullOrWhiteSpace(title), $"{url} needs a page title");
        Assert.Single(doc.QuerySelectorAll("h1"));
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task Page_has_a_skip_link_to_the_main_content(string url)
    {
        var doc = await LoadAsync(url);

        var firstLink = doc.Body!.QuerySelector("a");
        Assert.NotNull(firstLink);
        Assert.Equal("#main-content", firstLink!.GetAttribute("href"));
        Assert.NotNull(doc.QuerySelector("main#main-content"));
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task Every_image_has_alt_text(string url)
    {
        var doc = await LoadAsync(url);

        var missing = doc.QuerySelectorAll("img:not([alt])").Select(i => i.OuterHtml).ToList();
        Assert.True(missing.Count == 0, $"Images without alt on {url}: {string.Join(", ", missing)}");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task Every_button_and_link_has_an_accessible_name(string url)
    {
        var doc = await LoadAsync(url);

        var unnamed = doc.QuerySelectorAll("button, a[href]")
            .Where(e => !HasAccessibleName(e))
            .Select(e => e.OuterHtml)
            .ToList();
        Assert.True(unnamed.Count == 0, $"Unnamed controls on {url}: {string.Join(" | ", unnamed)}");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task Every_form_field_has_a_label(string url)
    {
        var doc = await LoadAsync(url);

        var unlabelled = doc.QuerySelectorAll("input:not([type=hidden]), select, textarea")
            .Where(f => f.GetAttribute("aria-label") is null
                        && (f.Id is null || doc.QuerySelector($"label[for='{f.Id}']") is null)
                        && f.Closest("label") is null)
            .Select(f => f.OuterHtml)
            .ToList();
        Assert.True(unlabelled.Count == 0, $"Unlabelled fields on {url}: {string.Join(" | ", unlabelled)}");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task Progress_bars_are_labelled(string url)
    {
        var doc = await LoadAsync(url);

        var unlabelled = doc.QuerySelectorAll("[role=progressbar]")
            .Where(p => p.GetAttribute("aria-label") is null && p.GetAttribute("aria-labelledby") is null)
            .Select(p => p.OuterHtml)
            .ToList();
        Assert.True(unlabelled.Count == 0, $"Unlabelled progress bars on {url}: {string.Join(" | ", unlabelled)}");
    }

    [Fact]
    public async Task Site_css_respects_reduced_motion_and_enforces_44px_tap_targets()
    {
        var css = await _app.CreateClient().GetStringAsync("/css/site.css");

        Assert.Contains("prefers-reduced-motion: reduce", css);
        Assert.Contains("min-height: 44px", css);
        Assert.Contains(":focus-visible", css);
    }

    /// <summary>
    /// Text counts only if it has a letter or digit, so icon-only buttons like "⟲" or "+" need an aria-label.
    /// </summary>
    private static bool HasAccessibleName(IElement e)
    {
        if (!string.IsNullOrWhiteSpace(e.GetAttribute("aria-label")) || e.GetAttribute("aria-labelledby") is not null)
            return true;
        var text = e.TextContent;
        if (text.Any(char.IsLetterOrDigit))
            return true;
        return e.QuerySelectorAll("img[alt]").Any(i => !string.IsNullOrWhiteSpace(i.GetAttribute("alt")));
    }
}
