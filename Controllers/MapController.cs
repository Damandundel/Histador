using Microsoft.AspNetCore.Mvc;

namespace Histador.Controllers
{
    [Route("[controller]")]
    public class MapController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
