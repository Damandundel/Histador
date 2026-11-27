using Microsoft.AspNetCore.Mvc;

namespace Histador.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressController : ControllerBase
    {
        /// <summary>
        /// Returns explored percentage per country keyed by ISO numeric code.
        /// Basic placeholder data for Issue #20 world map & fog of war presentation.
        /// </summary>
        [HttpGet("countries")]
        public IActionResult GetCountryProgress()
        {
            var countryProgress = new Dictionary<string, int>
            {
                { "840", 65 }, // United States
                { "250", 80 }, // France
                { "826", 50 }, // United Kingdom
                { "100", 90 }, // Bulgaria
                { "380", 75 }, // Italy
                { "300", 60 }, // Greece
                { "276", 40 }, // Germany
                { "724", 30 }, // Spain
                { "818", 55 }  // Egypt
            };

            return Ok(countryProgress);
        }
    }
}
