using Microsoft.AspNetCore.Identity;

namespace Histador.Models
{
    public class ApplicationUser : IdentityUser
    {
        public const int DisplayNameMinLength = 3;
        public const int DisplayNameMaxLength = 20;
        public const int StartingMmr = 1000;

        /// <summary>Public, unique (case-insensitive) name that friends search for.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>"bg" or "en".</summary>
        public string PreferredLanguage { get; set; } = SupportedLanguages.Default;

        /// <summary>Earned through perfect quizzes, spent on hints in duels.</summary>
        public int HintPoints { get; set; }

        public int Mmr { get; set; } = StartingMmr;

        /// <summary>Player icon; the Icon table arrives with the icons issue.</summary>
        public int? SelectedIconId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class SupportedLanguages
    {
        public const string English = "en";
        public const string Bulgarian = "bg";
        public const string Default = English;

        public static string Normalize(string? language) =>
            language?.Trim().ToLowerInvariant() is Bulgarian ? Bulgarian : English;
    }
}
