using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Histador.Data;
using Histador.Models;
using Histador.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Histador.Tests.FeedbackApi;

public class FeedbackApiTests : IClassFixture<HistadorAppFactory>
{
    private readonly HistadorAppFactory _app;
    private readonly HttpClient _client;

    public FeedbackApiTests(HistadorAppFactory app)
    {
        _app = app;
        _client = app.CreateClient();
    }

    [Fact]
    public async Task Feedback_can_be_submitted_without_login_and_returns_a_reference_number()
    {
        var content = $"The map takes too long to load ({Guid.NewGuid():N}).";

        var response = await _client.PostAsJsonAsync("/api/feedback", new { type = 1, content });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"referenceNumber\"", raw);
        Assert.Contains("\"message\"", raw);

        var result = JsonSerializer.Deserialize<FeedbackResponse>(
            raw, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(result);
        Assert.Matches(@"^FB-\d{6,}$", result!.ReferenceNumber);
        Assert.Equal("Thank you! Your feedback was received.", result.Message);

        await using var scope = _app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var saved = await db.Feedback.SingleAsync(f => f.Content == content);
        Assert.Equal(FeedbackType.Problem, saved.Type);
        Assert.Equal($"FB-{saved.Id:D6}", result.ReferenceNumber);
        Assert.InRange(saved.CreatedAt, DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(5));
    }

    [Fact]
    public async Task Content_is_trimmed_before_it_is_saved()
    {
        var content = $"  Please add a dark mode ({Guid.NewGuid():N}).  ";

        var response = await _client.PostAsJsonAsync("/api/feedback", new { type = 2, content });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = _app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var saved = await db.Feedback.SingleAsync(f => f.Content == content.Trim());
        Assert.Equal(FeedbackType.Suggestion, saved.Type);
    }

    [Theory]
    [InlineData(1, "too short")]
    [InlineData(2, "")]
    [InlineData(0, "This message is long enough.")]
    [InlineData(3, "This message is long enough.")]
    public async Task Invalid_feedback_is_rejected(int type, string content)
    {
        var response = await _client.PostAsJsonAsync("/api/feedback", new { type, content });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Whitespace_only_content_is_rejected_without_saving()
    {
        await using var scope = _app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var before = await db.Feedback.CountAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/feedback",
            new { type = 1, content = new string(' ', 50) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, await db.Feedback.CountAsync());
    }
}
