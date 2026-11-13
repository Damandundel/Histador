using Microsoft.AspNetCore.Mvc;

namespace Histador.Controllers
{
    // Placeholder pages for the main sections in the top nav.
    // Each feature issue replaces its controller with the real one (and deletes it from this file).

    public class MapController : Controller
    {
        public IActionResult Index() => View("ComingSoon", "Map");
    }

    public class LessonsController : Controller
    {
        public IActionResult Index() => View("ComingSoon", "Lessons");
    }

    public class RandomController : Controller
    {
        public IActionResult Index() => View("ComingSoon", "Random lesson");
    }

    public class DuelsController : Controller
    {
        public IActionResult Index() => View("ComingSoon", "Duels");
    }

    public class ProfileController : Controller
    {
        public IActionResult Index() => View("ComingSoon", "Profile");
    }
}
