namespace Histador.Models
{
    /// <summary>The JSON returned by POST /api/feedback.</summary>
    public sealed record FeedbackResponse(string ReferenceNumber, string Message)
    {
        /// <summary>Example: FB-000042 for feedback with Id 42.</summary>
        public static FeedbackResponse ForId(int id) =>
            new($"FB-{id:D6}", "Thank you! Your feedback was received.");
    }
}
