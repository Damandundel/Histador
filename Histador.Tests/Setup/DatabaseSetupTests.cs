using Histador.Data;
using Microsoft.EntityFrameworkCore;

namespace Histador.Tests.Setup;

public class DatabaseSetupTests
{
    [Fact]
    public void Database_schema_is_defined_by_migrations()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Unused") // never opened; only reads the migration list
            .Options;
        using var db = new ApplicationDbContext(options);

        Assert.NotEmpty(db.Database.GetMigrations());
    }
}
