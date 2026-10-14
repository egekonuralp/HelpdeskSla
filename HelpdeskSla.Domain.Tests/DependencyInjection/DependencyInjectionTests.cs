using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpdeskSla.Domain.Tests.DependencyInjection;

public class DependencyInjectionTests
{
    [Fact]
    public void ServiceProvider_ShouldResolveTicketService()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddTransient<ITicketRepository, FakeTicketRepository>();
        services.AddTransient<TicketService>();

        using var provider = services.BuildServiceProvider();

        // Act
        var service = provider.GetRequiredService<TicketService>();

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void SingletonRepository_ShouldReturnSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton<ITicketRepository, FakeTicketRepository>();

        using var provider = services.BuildServiceProvider();

        // Act
        var repository1 = provider.GetRequiredService<ITicketRepository>();
        var repository2 = provider.GetRequiredService<ITicketRepository>();

        // Assert
        Assert.Same(repository1, repository2);
    }

    [Fact]
    public void ScopedRepository_DifferentScopes_ShouldReturnDifferentInstances()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddScoped<ITicketRepository, FakeTicketRepository>();

        using var provider = services.BuildServiceProvider();
        
        // Act
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var repo1 = scope1.ServiceProvider.GetRequiredService<ITicketRepository>();
        var repo2 = scope2.ServiceProvider.GetRequiredService<ITicketRepository>();

        // Assert
        Assert.NotSame(repo1, repo2);
    }

    [Fact]
    public void TransientRepository_SameScope_ShouldReturnDifferentInstances()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddTransient<ITicketRepository, FakeTicketRepository>();

        using var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();

        var repo1 = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var repo2 = scope.ServiceProvider.GetRequiredService<ITicketRepository>();

        // Assert
        Assert.NotSame(repo1, repo2);
    }
}
