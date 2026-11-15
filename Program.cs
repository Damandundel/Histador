using Histador.Data;
using Microsoft.EntityFrameworkCore;

namespace Histador
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

            builder.Services.AddControllersWithViews();

            // Error logging and request telemetry. Turns on when APPLICATIONINSIGHTS_CONNECTION_STRING
            // is configured (it is on Azure App Service); locally it stays off.
            builder.Services.AddApplicationInsightsTelemetry();

            // GET /health: 200 "Healthy" when the app is up and can reach the database.
            // Azure App Service pings it (see infra/main.bicep) and so does the deploy workflow.
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>("database");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapHealthChecks("/health");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
