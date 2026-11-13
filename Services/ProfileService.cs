using Histador.Identity;
using Histador.Models;
using Histador.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Histador.Services
{
    public class ProfileService(UserManager<ApplicationUser> users)
    {
        public Task<ApplicationUser?> FindByDisplayNameAsync(string displayName)
        {
            var upper = displayName.Trim().ToUpperInvariant();
            return users.Users.FirstOrDefaultAsync(u => u.DisplayName.ToUpper() == upper);
        }

        public async Task<ProfileViewModel> BuildAsync(ApplicationUser user, bool isOwnProfile)
        {
            var worldPercent = await GetWorldPercentAsync(user.Id);

            return new ProfileViewModel
            {
                IsOwnProfile = isOwnProfile,
                DisplayName = user.DisplayName,
                Initials = GetInitials(user.DisplayName),
                SelectedIconId = user.SelectedIconId,
                IsAdmin = await users.IsInRoleAsync(user, Roles.Admin),
                WorldPercent = worldPercent,
                Level = (int)Math.Floor(worldPercent),
                Rank = null,
                Mmr = user.Mmr,
                HintPoints = isOwnProfile ? user.HintPoints : null,
                LessonsPassed = null,
                PerfectQuizzes = null,
                DuelsWon = null,
                DuelsLost = null,
            };
        }

        private static Task<double> GetWorldPercentAsync(string userId) => Task.FromResult(0d);

        private static string GetInitials(string displayName)
        {
            var letters = displayName.Where(char.IsLetter).Take(2).ToArray();
            return letters.Length == 0 ? "?" : new string(letters).ToUpperInvariant();
        }
    }
}
