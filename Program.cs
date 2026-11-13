using Histador.Data;
using Histador.Services;
using Microsoft.EntityFrameworkCore;

namespace Histador
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<HistadorDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("Histador")));
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<CurrentPlayerService>();
            builder.Services.AddScoped<MapStateService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<HistadorDbContext>();
                db.Database.EnsureCreated();
                SeedData.Seed(db);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            var cultures = new[] { "bg", "en" };
            app.UseRequestLocalization(new RequestLocalizationOptions()
                .SetDefaultCulture("bg")
                .AddSupportedCultures(cultures)
                .AddSupportedUICultures(cultures));

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
