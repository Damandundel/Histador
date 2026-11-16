using System.Net;
using System.Text.RegularExpressions;

namespace Histador.Tests.Support;

/// <summary>Helpers for submitting the app's real HTML forms (with their anti-forgery token).</summary>
public static partial class FormClient
{
    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();

    public static async Task<HttpResponseMessage> SubmitAsync(
        this HttpClient client, string pageUrl, IDictionary<string, string> fields, string? postUrl = null)
    {
        var page = await client.GetAsync(pageUrl);
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var html = await page.Content.ReadAsStringAsync();
        var token = TokenRegex().Match(html);
        Assert.True(token.Success, $"No anti-forgery token on {pageUrl}");

        var form = new Dictionary<string, string>(fields) { ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token.Groups[1].Value) };
        return await client.PostAsync(postUrl ?? pageUrl, new FormUrlEncodedContent(form));
    }

    public static Task<HttpResponseMessage> LoginAsync(this HttpClient client, string email, string password) =>
        client.SubmitAsync("/Account/Login", new Dictionary<string, string> { ["Email"] = email, ["Password"] = password });

    public static Task<HttpResponseMessage> RegisterAsync(this HttpClient client, string email, string displayName, string password = "Passw0rd!") =>
        client.SubmitAsync("/Account/Register", new Dictionary<string, string>
        {
            ["Email"] = email,
            ["DisplayName"] = displayName,
            ["Password"] = password,
            ["ConfirmPassword"] = password,
            ["PreferredLanguage"] = "en",
        });
}
