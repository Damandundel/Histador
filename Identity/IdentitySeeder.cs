using Histador.Models;
using Microsoft.AspNetCore.Identity;

namespace Histador.Identity
{
    /// <summary>
    /// Creates the roles and, when "AdminSeed:Email" and "AdminSeed:Password" are configured
    /// (user-secrets locally, app settings in production), one admin account. Safe to run on every start.
    /// </summary>
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            await using var scope = services.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var roles = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var users = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(IdentitySeeder));

            foreach (var role in Roles.All)
            {
                if (!await roles.RoleExistsAsync(role))
                    await roles.CreateAsync(new IdentityRole(role));
            }

            var email = config["AdminSeed:Email"];
            var password = config["AdminSeed:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogInformation("AdminSeed:Email/Password not configured; no admin account seeded.");
                return;
            }

            var admin = await users.FindByEmailAsync(email);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    DisplayName = config["AdminSeed:DisplayName"] ?? "Admin",
                };
                var created = await users.CreateAsync(admin, password);
                if (!created.Succeeded)
                {
                    logger.LogError("Could not seed admin: {Errors}", string.Join("; ", created.Errors.Select(e => e.Description)));
                    return;
                }
            }

            foreach (var role in Roles.All)
            {
                if (!await users.IsInRoleAsync(admin, role))
                    await users.AddToRoleAsync(admin, role);
            }
        }
    }
}
