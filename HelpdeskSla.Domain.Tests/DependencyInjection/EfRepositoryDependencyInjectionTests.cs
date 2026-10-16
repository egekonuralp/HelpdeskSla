using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Infrastructure.Persistence;
using HelpdeskSla.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpdeskSla.Domain.Tests.DependencyInjection;

public class EfRepositoryDependencyInjectionTests
{
    [Fact]
    public void ServiceProvider_ResolvesTicketService_WithEfRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddDbContext<HelpdeskDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<ITicketRepository, EfTicketRepository>();
        services.AddScoped<TicketService>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act
        var ticketService = scope.ServiceProvider.GetRequiredService<TicketService>();

        // Assert
        Assert.NotNull(ticketService);
    }
}