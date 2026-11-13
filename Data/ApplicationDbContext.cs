using Microsoft.EntityFrameworkCore;

namespace Histador.Data
{
    /// <summary>
    /// The app's database. Entities are added by the feature issues (accounts, lessons, map, duels),
    /// each with its own migration: <c>dotnet ef migrations add &lt;Name&gt; --output-dir Data/Migrations</c>.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }
}
