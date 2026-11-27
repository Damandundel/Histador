using Histador.Data;
using Histador.Models;
using Histador.Services;
using Microsoft.AspNetCore.Identity;

namespace Histador.Identity
{
    public static class IdentityServiceCollectionExtensions
    {
        /// <summary>Registers Identity exactly as the app uses it (also used by the tests).</summary>
        public static IServiceCollection AddHistadorIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequiredLength = 8;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddUserValidator<DisplayNameValidator>()
                .AddClaimsPrincipalFactory<HistadorClaimsPrincipalFactory>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                // Logged-in users without the right role get a real 403 (rendered by the status page).
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

            services.AddScoped<AccountService>();
            return services;
        }
    }
}
