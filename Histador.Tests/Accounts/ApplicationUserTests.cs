using Histador.Models;

namespace Histador.Tests.Accounts;

public class ApplicationUserTests
{
    [Fact]
    public void New_user_starts_with_1000_mmr_and_no_hint_points()
    {
        var user = new ApplicationUser();

        Assert.Equal(1000, user.Mmr);
        Assert.Equal(0, user.HintPoints);
    }

    [Fact]
    public void New_user_has_english_as_default_language_and_no_icon()
    {
        var user = new ApplicationUser();

        Assert.Equal("en", user.PreferredLanguage);
        Assert.Null(user.SelectedIconId);
    }
}
