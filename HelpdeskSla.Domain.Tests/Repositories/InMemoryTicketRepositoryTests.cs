using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Domain.Tests.Fakes;
using HelpdeskSla.Infrastructure.Repositories;
using Xunit;

namespace HelpdeskSla.Domain.Tests.Repositories;

public class InMemoryTicketRepositoryTests
{
    [Fact]
    public void Add_ValidTicket_ShouldStoreTicket()
    {
        // Arrange
        var repository = new InMemoryTicketRepository();

        var ticket = new Ticket(
            "Sunucu çalışmıyor",
            "Sunucuya erişilemiyor",
            TicketPriority.High);

        // Act
        repository.Add(ticket);
        var tickets = repository.GetAll();

        // Assert
        Assert.Single(tickets);
        Assert.Same(ticket, tickets[0]);
    }

    [Fact]
    public async Task CreateTicketAsync_InMemoryRepository_ShouldStoreTicket()
    {
        // Arrange
        var repository = new InMemoryTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var ticket = await service.CreateTicketAsync(
            "Sunucu çalışmıyor",
            "Sunucuya erişilemiyor",
            TicketPriority.High);

        var tickets = repository.GetAll();

        // Assert
        Assert.Single(tickets);
        Assert.Same(ticket, tickets[0]);
    }
}