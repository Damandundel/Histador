using Histador.Data;
using Histador.Identity;
using Histador.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Histador
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

            builder.Services.AddHistadorIdentity();
            builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();
            builder.Services.AddScoped<ProfileService>();

            // Everything requires login unless a controller/action is marked [AllowAnonymous].
            builder.Services.AddAuthorization(options =>
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            await InitializeDatabaseAsync(app);

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }

        /// <summary>
        /// "Database:Initialize" = Migrate (default) | EnsureCreated (tests) | None. Then seeds roles and the admin.
        /// </summary>
        private static async Task InitializeDatabaseAsync(WebApplication app)
        {
            var mode = app.Configuration["Database:Initialize"] ?? "Migrate";
            if (!mode.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                await using var scope = app.Services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (mode.Equals("EnsureCreated", StringComparison.OrdinalIgnoreCase))
                    await db.Database.EnsureCreatedAsync();
                else
                    await db.Database.MigrateAsync();
            }

            await IdentitySeeder.SeedAsync(app.Services);
        }
    }
}
