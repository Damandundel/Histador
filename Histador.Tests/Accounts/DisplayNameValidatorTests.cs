using Histador.Models;
using Histador.Tests.Support;

namespace Histador.Tests.Accounts;

public class DisplayNameValidatorTests
{
    private static ApplicationUser NewUser(string displayName, string email) =>
        new() { UserName = email, Email = email, DisplayName = displayName };

    [Fact]
    public async Task Duplicate_display_name_is_rejected_ignoring_case()
    {
        await using var host = new IdentityTestHost();
        await using var scope = host.Scope();
        var users = IdentityTestHost.Users(scope);
        Assert.True((await users.CreateAsync(NewUser("Cleopatra", "a@test.com"), "Passw0rd!")).Succeeded);

        var result = await users.CreateAsync(NewUser("cleopatra", "b@test.com"), "Passw0rd!");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "DuplicateDisplayName");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("abcdefghijklmnopqrstu")] // 21 chars
    [InlineData("   ")]
    public async Task Display_name_outside_3_to_20_characters_is_rejected(string name)
    {
        await using var host = new IdentityTestHost();
        await using var scope = host.Scope();

        var result = await IdentityTestHost.Users(scope).CreateAsync(NewUser(name, "a@test.com"), "Passw0rd!");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "InvalidDisplayNameLength");
    }

    [Theory]
    [InlineData("Leo")]
    [InlineData("abcdefghijklmnopqrst")] // 20 chars
    public async Task Display_name_within_bounds_is_accepted(string name)
    {
        await using var host = new IdentityTestHost();
        await using var scope = host.Scope();

        var result = await IdentityTestHost.Users(scope).CreateAsync(NewUser(name, "a@test.com"), "Passw0rd!");

        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Code)));
    }

    [Fact]
    public async Task Updating_a_user_keeps_their_own_display_name_valid()
    {
        await using var host = new IdentityTestHost();
        await using var scope = host.Scope();
        var users = IdentityTestHost.Users(scope);
        var user = NewUser("Leonidas", "a@test.com");
        await users.CreateAsync(user, "Passw0rd!");

        user.PreferredLanguage = "bg";
        var result = await users.UpdateAsync(user);

        Assert.True(result.Succeeded);
    }
}
