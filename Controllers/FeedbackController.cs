using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Histador.Controllers
{
    /// <summary>
    /// The feedback form is a public shell. It posts JSON to /api/feedback,
    /// which must also be [AllowAnonymous] so this page works for visitors.
    /// </summary>
    [AllowAnonymous]
    public class FeedbackController : Controller
    {
        public IActionResult Index() => View();
    }
}
