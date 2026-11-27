using Histador.Data;
using Histador.Models;
using Histador.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Histador.Tests.FeedbackData;

/// <summary>The database part of feedback: Feedback { Id, Type, Content, CreatedAt } and its migration.</summary>
public class FeedbackStorageTests
{
    private static ApplicationDbContext Db(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    [Fact]
    public void Feedback_types_are_1_for_problem_and_2_for_suggestion()
    {
        Assert.Equal(1, (int)FeedbackType.Problem);
        Assert.Equal(2, (int)FeedbackType.Suggestion);
    }

    [Theory]
    [InlineData(FeedbackType.Problem)]
    [InlineData(FeedbackType.Suggestion)]
    public async Task Feedback_is_saved_and_read_back(FeedbackType type)
    {
        await using var host = new IdentityTestHost();
        await using var scope = host.Scope();
        var createdAt = new DateTime(2026, 11, 27, 9, 45, 0, DateTimeKind.Utc);

        var entry = new Feedback { Type = type, Content = "The map takes a long time to load.", CreatedAt = createdAt };
        Db(scope).Feedback.Add(entry);
        await Db(scope).SaveChangesAsync();

        Assert.True(entry.Id > 0, "The database gives each feedback an Id");
        var saved = await Db(scope).Feedback.AsNoTracking().SingleAsync();
        Assert.Equal(entry.Id, saved.Id);
        Assert.Equal(type, saved.Type);
        Assert.Equal("The map takes a long time to load.", saved.Content);
        Assert.Equal(createdAt, saved.CreatedAt);
    }

    [Fact]
    public void CreatedAt_is_filled_in_automatically()
    {
        var before = DateTime.UtcNow.AddSeconds(-5);

        var entry = new Feedback { Type = FeedbackType.Problem, Content = "Something is broken." };

        Assert.True(entry.CreatedAt >= before && entry.CreatedAt <= DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task The_type_is_stored_as_a_number()
    {
        await using var host = new IdentityTestHost();
        await using var scope = host.Scope();
        Db(scope).Feedback.Add(new Feedback { Type = FeedbackType.Suggestion, Content = "Please add more lessons." });
        await Db(scope).SaveChangesAsync();

        var storedType = await Db(scope).Database
            .SqlQueryRaw<int>("SELECT \"Type\" AS \"Value\" FROM \"Feedback\"")
            .SingleAsync();

        Assert.Equal(2, storedType);
    }

    [Fact]
    public async Task Newest_feedback_can_be_listed_first()
    {
        await using var host = new IdentityTestHost();
        await using var scope = host.Scope();
        Db(scope).Feedback.AddRange(
            new Feedback { Type = FeedbackType.Problem, Content = "Older message here.", CreatedAt = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Feedback { Type = FeedbackType.Suggestion, Content = "Newer message here.", CreatedAt = new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc) });
        await Db(scope).SaveChangesAsync();

        var newestFirst = await Db(scope).Feedback.OrderByDescending(f => f.CreatedAt).Select(f => f.Content).ToListAsync();

        Assert.Equal(new[] { "Newer message here.", "Older message here." }, newestFirst);
    }

    [Fact]
    public async Task Columns_have_the_agreed_rules()
    {
        await using var host = new IdentityTestHost();
        await using var scope = host.Scope();
        var entity = Db(scope).Model.FindEntityType(typeof(Feedback))!;

        Assert.Equal(new[] { "Content", "CreatedAt", "Id", "Type" }, entity.GetProperties().Select(p => p.Name).OrderBy(n => n));

        var content = entity.FindProperty(nameof(Feedback.Content))!;
        Assert.Equal(1000, content.GetMaxLength());
        Assert.False(content.IsNullable);
        Assert.Equal(10, Feedback.ContentMinLength);
        Assert.Equal(1000, Feedback.ContentMaxLength);
    }

    [Fact]
    public async Task The_AddFeedback_migration_is_registered()
    {
        await using var host = new IdentityTestHost();
        await using var scope = host.Scope();

        Assert.Contains("20261127093000_AddFeedback", Db(scope).Database.GetMigrations());
    }
}
