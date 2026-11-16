using System.Security.Claims;
using Histador.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Histador.Identity
{
    /// <summary>Adds the display name to the login cookie so views can show it without a database call.</summary>
    public class HistadorClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
    {
        public const string DisplayNameClaim = "display_name";

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(DisplayNameClaim, user.DisplayName));
            return identity;
        }
    }
}
