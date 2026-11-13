using Histador.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Histador.Identity
{
    /// <summary>Display names must be 3–20 characters and unique, ignoring case.</summary>
    public class DisplayNameValidator : IUserValidator<ApplicationUser>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var name = user.DisplayName?.Trim() ?? string.Empty;

            if (name.Length < ApplicationUser.DisplayNameMinLength || name.Length > ApplicationUser.DisplayNameMaxLength)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidDisplayNameLength",
                    Description = $"Display name must be {ApplicationUser.DisplayNameMinLength}–{ApplicationUser.DisplayNameMaxLength} characters.",
                });
            }

            var upper = name.ToUpperInvariant();
            var taken = await manager.Users.AnyAsync(u => u.Id != user.Id && u.DisplayName.ToUpper() == upper);
            if (taken)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateDisplayName",
                    Description = $"Display name '{name}' is already taken.",
                });
            }

            return IdentityResult.Success;
        }
    }
}
