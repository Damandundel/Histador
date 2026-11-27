using Histador.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Histador.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(user =>
            {
                user.Property(u => u.DisplayName).HasMaxLength(ApplicationUser.DisplayNameMaxLength).IsRequired();
                user.HasIndex(u => u.DisplayName).IsUnique();
                user.Property(u => u.PreferredLanguage).HasMaxLength(2).IsRequired();
                user.Property(u => u.Mmr).HasDefaultValue(ApplicationUser.StartingMmr);
            });
        }
    }
}
