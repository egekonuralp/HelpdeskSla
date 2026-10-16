using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HelpdeskSla.Infrastructure.Persistence;

public class HelpdeskDbContextFactory : IDesignTimeDbContextFactory<HelpdeskDbContext>
{
    public HelpdeskDbContext CreateDbContext(string[] args)
    {
        var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=HelpdeskSlaDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new HelpdeskDbContext(options);
    }
}