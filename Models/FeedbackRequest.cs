using System.ComponentModel.DataAnnotations;

namespace Histador.Models
{
    /// <summary>The JSON body of POST /api/feedback. Not a database entity.</summary>
    public class FeedbackRequest
    {
        [EnumDataType(typeof(FeedbackType), ErrorMessage = "Choose Problem or Suggestion.")]
        public FeedbackType Type { get; set; }

        [Required(ErrorMessage = "Enter a message.")]
        [StringLength(Feedback.ContentMaxLength, MinimumLength = Feedback.ContentMinLength,
            ErrorMessage = "The message must be between 10 and 1000 characters.")]
        public string Content { get; set; } = string.Empty;
    }
}
