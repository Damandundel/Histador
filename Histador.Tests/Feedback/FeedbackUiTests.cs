using System.Net;
using Histador.Tests.Support;

namespace Histador.Tests.Feedback;

public class FeedbackUiTests : IClassFixture<HistadorAppFactory>
{
    private readonly HttpClient _client;

    public FeedbackUiTests(HistadorAppFactory app) => _client = app.CreateClient();

    [Fact]
    public async Task Feedback_page_is_public_and_renders_the_form()
    {
        var response = await _client.GetAsync("/Feedback");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("id=\"feedback-form\"", html);
        Assert.Contains("id=\"feedback-submit\"", html);
        Assert.Contains("name=\"type\"", html);
        Assert.Contains("value=\"1\"", html);
        Assert.Contains("value=\"2\"", html);
        Assert.Contains("name=\"content\"", html);
        Assert.Contains("maxlength=\"1000\"", html);
        Assert.Contains("0 / 1000", html);
        Assert.Contains("/js/feedback.js", html);
    }

    [Fact]
    public async Task Footer_links_to_the_feedback_page()
    {
        var html = await _client.GetStringAsync("/");

        Assert.Contains("href=\"/Feedback\"", html);
    }
}
