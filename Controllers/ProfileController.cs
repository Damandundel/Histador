using Histador.Models;
using Histador.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Histador.Controllers
{
    [Route("Profile")]
    public class ProfileController(UserManager<ApplicationUser> users, ProfileService profiles) : Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var me = await users.GetUserAsync(User);
            if (me is null)
                return Challenge();

            return View("Index", await profiles.BuildAsync(me, isOwnProfile: true));
        }

        [HttpGet("{displayName}")]
        public async Task<IActionResult> Show(string displayName)
        {
            var player = await profiles.FindByDisplayNameAsync(displayName);
            if (player is null)
                return NotFound();

            var isOwnProfile = player.Id == users.GetUserId(User);
            return View("Index", await profiles.BuildAsync(player, isOwnProfile));
        }
    }
}
