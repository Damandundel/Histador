using System.Net;
using System.Text.RegularExpressions;
using Histador.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Histador.Tests.Accounts;

public class AccountFlowTests : IClassFixture<HistadorAppFactory>
{
    private readonly HistadorAppFactory _app;

    public AccountFlowTests(HistadorAppFactory app) => _app = app;

    private HttpClient NewClient() =>
        _app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static string UniqueName() => "U" + Guid.NewGuid().ToString("N")[..10];

    [Fact]
    public async Task Landing_page_is_public_and_invites_visitors_to_start_learning()
    {
        var response = await NewClient().GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Start learning", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    [InlineData("/Account/ForgotPassword")]
    public async Task Account_entry_pages_are_public(string url)
    {
        var response = await NewClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/Account/ChangePassword")]
    [InlineData("/Admin")]
    [InlineData("/Map")]
    [InlineData("/Profile")]
    public async Task Other_pages_send_anonymous_visitors_to_login(string url)
    {
        var response = await NewClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Headers.Location!.PathAndQuery);
    }

    [Fact]
    public async Task Admin_routes_return_403_for_players()
    {
        var client = NewClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "player-1");
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, "Player");

        var response = await client.GetAsync("/Admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Seeded_admin_can_log_in_and_open_admin_area()
    {
        var client = NewClient();

        var login = await client.LoginAsync(HistadorAppFactory.AdminEmail, HistadorAppFactory.AdminPassword);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var admin = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.OK, admin.StatusCode);
    }

    [Fact]
    public async Task New_player_can_register_log_out_and_log_back_in()
    {
        var client = NewClient();
        var email = $"{UniqueName()}@test.com";

        var register = await client.RegisterAsync(email, UniqueName());
        Assert.Equal(HttpStatusCode.Redirect, register.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Account/ChangePassword")).StatusCode);

        var logout = await client.SubmitAsync("/Account/ChangePassword", new Dictionary<string, string>(), "/Account/Logout");
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Account/ChangePassword")).StatusCode);

        var login = await client.LoginAsync(email, "Passw0rd!");
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Account/ChangePassword")).StatusCode);
    }

    [Fact]
    public async Task Registering_with_a_taken_display_name_shows_a_clear_message()
    {
        var name = UniqueName();
        await NewClient().RegisterAsync($"{UniqueName()}@test.com", name);

        var second = await NewClient().RegisterAsync($"{UniqueName()}@test.com", name.ToLowerInvariant());

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Contains("is already taken", await second.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Wrong_password_does_not_log_in()
    {
        var client = NewClient();

        var login = await client.LoginAsync(HistadorAppFactory.AdminEmail, "wrong-password");

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Contains("Invalid email or password", await login.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Player_can_reset_a_forgotten_password_via_the_emailed_link()
    {
        var email = $"{UniqueName()}@test.com";
        await NewClient().RegisterAsync(email, UniqueName());
        var client = NewClient();

        var forgot = await client.SubmitAsync("/Account/ForgotPassword", new Dictionary<string, string> { ["Email"] = email });
        Assert.Equal(HttpStatusCode.Redirect, forgot.StatusCode);

        var mail = Assert.Single(_app.Outbox.Sent, m => m.To == email);
        var link = Regex.Match(mail.HtmlBody, "href=\"([^\"]+)\"").Groups[1].Value;
        var resetUrl = new Uri(WebUtility.HtmlDecode(link)).PathAndQuery;
        var code = Uri.UnescapeDataString(Regex.Match(resetUrl, "code=([^&]+)").Groups[1].Value);

        var reset = await client.SubmitAsync(resetUrl, new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = "N3w-Passw0rd!",
            ["ConfirmPassword"] = "N3w-Passw0rd!",
            ["Code"] = code,
        }, "/Account/ResetPassword");
        Assert.Equal(HttpStatusCode.Redirect, reset.StatusCode);

        var login = await client.LoginAsync(email, "N3w-Passw0rd!");
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
    }

    [Fact]
    public async Task Forgot_password_for_unknown_email_does_not_reveal_anything()
    {
        var forgot = await NewClient().SubmitAsync("/Account/ForgotPassword",
            new Dictionary<string, string> { ["Email"] = "nobody@test.com" });

        Assert.Equal(HttpStatusCode.Redirect, forgot.StatusCode);
        Assert.DoesNotContain(_app.Outbox.Sent, m => m.To == "nobody@test.com");
    }
}
