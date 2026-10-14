using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace DeluxeNET.Data
{
    // Used by `dotnet ef` at design time so migrations can be created without
    // a live database. The runtime path in Program.cs still auto-detects the
    // server version from the real connection string.
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>();
            options.UseMySql(
                "server=localhost;database=design_time;user=root;password=;",
                new MySqlServerVersion(new Version(8, 0, 0))
            );
            return new AppDbContext(options.Options);
        }
    }
}
