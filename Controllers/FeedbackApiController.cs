using Histador.Models;
using Histador.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Histador.Controllers
{
    /// <summary>JSON endpoint used by the public feedback form.</summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/feedback")]
    public class FeedbackApiController(
        IFeedbackService feedback,
        ILogger<FeedbackApiController> logger) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(FeedbackResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FeedbackResponse>> Create(
            [FromBody] FeedbackRequest request,
            CancellationToken cancellationToken)
        {
            // Model validation already checked the raw 10-1000 range.
            // Trim here so a message of only spaces cannot get through.
            var content = (request.Content ?? string.Empty).Trim();
            if (content.Length < Feedback.ContentMinLength)
            {
                ModelState.AddModelError(
                    nameof(request.Content),
                    $"The message must be at least {Feedback.ContentMinLength} characters.");
                return ValidationProblem(ModelState);
            }

            try
            {
                return Ok(await feedback.SubmitAsync(request.Type, content, cancellationToken));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save feedback.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Could not save feedback. Please try again.");
            }
        }
    }
}
