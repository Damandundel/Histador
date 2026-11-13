using Histador.Identity;
using Histador.Models;
using Microsoft.AspNetCore.Identity;

namespace Histador.Services
{
    public class AccountService(UserManager<ApplicationUser> users)
    {
        /// <summary>Creates a player account. Every new account gets the Player role.</summary>
        public async Task<IdentityResult> RegisterAsync(string email, string displayName, string password, string? language)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName.Trim(),
                PreferredLanguage = SupportedLanguages.Normalize(language),
            };

            var created = await users.CreateAsync(user, password);
            if (!created.Succeeded)
                return created;

            return await users.AddToRoleAsync(user, Roles.Player);
        }
    }
}
