using Histador.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Histador.Controllers
{
    /// <summary>Admin area entry point. The lesson/question editor (its own issue) builds on this.</summary>
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        public IActionResult Index() => View();
    }
}
