using HelpdeskSla.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Xunit;

namespace HelpdeskSla.Domain.Tests.Persistence;

public class SqlServerConnectionTests
{
    [Fact]
    public void SqlServer_ShouldConnectSuccessfully()
    {
        // Arrange
        var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new HelpdeskDbContext(options);

        // Act
        var canConnect = context.Database.CanConnect();

        // Assert
        Assert.True(canConnect);
    }

    [Fact]
    public void SqlServer_ConnectionString_ShouldTargetMasterDatabase()
    {
        // Arrange
        var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"; 

        // Act
        var builder = new SqlConnectionStringBuilder(connectionString);

        // Assert
        Assert.Equal("master", builder.InitialCatalog);
    }
}