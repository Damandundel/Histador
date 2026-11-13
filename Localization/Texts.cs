using System.Globalization;

namespace Histador.Localization;

public static class Texts
{
    private static readonly Dictionary<string, (string Bg, string En)> All = new()
    {
        ["nav_profile"] = ("Профил", "Profile"),
        ["profile_title"] = ("Профил", "Profile"),
        ["your_profile"] = ("Твоят профил", "Your profile"),
        ["admin"] = ("Админ", "Admin"),
        ["level"] = ("Ниво {0}", "Level {0}"),
        ["xp"] = ("{0} XP", "{0} XP"),
        ["world_discovered"] = ("{0}% от света е открит", "{0}% of the world discovered"),
        ["rank"] = ("Ранг", "Rank"),
        ["mmr"] = ("MMR", "MMR"),
        ["unranked"] = ("Без ранг", "Unranked"),
        ["hint_points"] = ("Точки за подсказки", "Hint points"),
        ["lessons_passed"] = ("Минати уроци", "Lessons passed"),
        ["perfect_quizzes"] = ("Перфектни тестове", "Perfect quizzes"),
        ["duels_won"] = ("Спечелени дуели", "Duels won"),
        ["duels_lost"] = ("Загубени дуели", "Duels lost"),
        ["add_friend"] = ("Добави приятел", "Add friend"),
        ["challenge"] = ("Предизвикай на дуел", "Challenge to duel"),
        ["coming_soon"] = ("Скоро", "Coming soon"),
        ["map_owned"] = ("Твои", "Owned"),
        ["map_contested"] = ("Спорни", "Contested"),
        ["map_discovered"] = ("Открити", "Discovered"),
        ["map_fog"] = ("Неоткрити", "Undiscovered"),
        ["mini_map"] = ("Мини карта", "Mini-map"),
    };

    public static bool IsEnglish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en";

    public static string T(string key, params object[] args)
    {
        if (!All.TryGetValue(key, out var text)) return key;
        var value = IsEnglish ? text.En : text.Bg;
        return args.Length == 0 ? value : string.Format(value, args);
    }
}
