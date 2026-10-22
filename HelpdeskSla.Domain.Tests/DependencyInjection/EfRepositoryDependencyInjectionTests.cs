using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Infrastructure.Persistence;
using HelpdeskSla.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static System.Formats.Asn1.AsnWriter;

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
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<TicketService>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act
        var ticketService = scope.ServiceProvider.GetRequiredService<TicketService>();

        // Assert
        Assert.NotNull(ticketService);
    }

    [Fact]
    public void ScopedDbContext_SameScope_ReturnsSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddDbContext<HelpdeskDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        using var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();

        var context1 = scope.ServiceProvider.GetRequiredService<HelpdeskDbContext>();
        var context2 = scope.ServiceProvider.GetRequiredService<HelpdeskDbContext>();

        // Assert
        Assert.Same(context1, context2);
    }

    [Fact]
    public void ScopedDbContext_DifferentScopes_ReturnsDifferentInstances()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddDbContext<HelpdeskDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        using var provider = services.BuildServiceProvider();

        // Act
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var context1 = scope1.ServiceProvider.GetRequiredService<HelpdeskDbContext>();
        var context2 = scope2.ServiceProvider.GetRequiredService<HelpdeskDbContext>();

        // Assert
        Assert.NotSame(context1, context2);
    }

    [Fact]
    public async Task ScopedRepositoryAndUnitOfWork_SameScope_PersistsTicket()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();

        services.AddDbContext<HelpdeskDbContext>(options => options.UseInMemoryDatabase(databaseName));

        services.AddScoped<ITicketRepository, EfTicketRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        using var provider = services.BuildServiceProvider();

        var ticket = new Ticket(
            "Sunucu Hatası",
            "Sunucu yanıt vermiyor.",
            TicketPriority.High);

        // Act
        using (var scope = provider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await repository.AddAsync(ticket);
            await unitOfWork.SaveChangesAsync();
        }

        using (var scope2 = provider.CreateScope())
        {
            var context2 = scope2.ServiceProvider.GetRequiredService<HelpdeskDbContext>();

            var savedTicket = await context2.Tickets
                .AsNoTracking()
                .SingleAsync(t => t.Id == ticket.Id);

            Assert.Equal("Sunucu Hatası", savedTicket.Title);
            Assert.Equal("Sunucu yanıt vermiyor.", savedTicket.Description);
            Assert.Equal(TicketPriority.High, savedTicket.Priority);
        }
    }

    [Fact]
    public void SingletonUnitOfWork_DependsOnScopedDbContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddDbContext<HelpdeskDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddSingleton<IUnitOfWork, EfUnitOfWork>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => 
            provider.GetRequiredService<IUnitOfWork>());

        // Assert
        Assert.Contains("scoped", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TransientTicketService_SameScope_ReturnsDifferentServices()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddDbContext<HelpdeskDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddScoped<ITicketRepository, EfTicketRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddTransient<TicketService>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        // Act
        using (var scope = provider.CreateScope())
        {
            var service1 = scope.ServiceProvider.GetRequiredService<TicketService>();
            var service2 = scope.ServiceProvider.GetRequiredService<TicketService>();

            var repository1 = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
            var repository2 = scope.ServiceProvider.GetRequiredService<ITicketRepository>();

            // Assert
            Assert.NotSame(service1, service2);
            Assert.Same(repository1, repository2);
        }
    }
}