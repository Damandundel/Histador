namespace Histador.Models
{
    /// <summary>What kind of feedback a player sends. Stored as a number: 1 or 2.</summary>
    public enum FeedbackType
    {
        Problem = 1,
        Suggestion = 2,
    }

    /// <summary>A problem report or suggestion a player sends about the app.</summary>
    public class Feedback
    {
        public const int ContentMinLength = 10;
        public const int ContentMaxLength = 1000;

        public int Id { get; set; }

        public FeedbackType Type { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
