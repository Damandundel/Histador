using Histador.Data;
using Histador.Models;

namespace Histador.Services
{
    public interface IFeedbackService
    {
        Task<FeedbackResponse> SubmitAsync(
            FeedbackType type,
            string content,
            CancellationToken cancellationToken = default);
    }

    public class FeedbackService(ApplicationDbContext db) : IFeedbackService
    {
        public async Task<FeedbackResponse> SubmitAsync(
            FeedbackType type,
            string content,
            CancellationToken cancellationToken = default)
        {
            var feedback = new Feedback
            {
                Type = type,
                Content = content,
                CreatedAt = DateTime.UtcNow,
            };

            db.Feedback.Add(feedback);
            await db.SaveChangesAsync(cancellationToken);

            return FeedbackResponse.ForId(feedback.Id);
        }
    }
}
