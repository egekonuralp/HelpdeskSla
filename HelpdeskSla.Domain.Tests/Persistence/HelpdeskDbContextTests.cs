using HelpdeskSla.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using Xunit;

namespace HelpdeskSla.Domain.Tests.Persistence;

public class HelpdeskDbContextTests
{
    [Fact]
    public void DbContext_ShouldCreateSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act
        using var context = new HelpdeskDbContext(options);

        // Assert
        Assert.NotNull(context);
    }

    [Fact]
    public void SaveChanges_ValidTicket_ShouldStoreTicket()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);

        var ticket = new Ticket(
            "Sunucu çalışmıyor",
            "Sunucuya erişilemiyor",
            TicketPriority.High);

        // Act
        context.Tickets.Add(ticket);
        context.SaveChanges();

        // Assert
        var savedTicket = context.Tickets.Single();

        Assert.Equal(ticket.Title, savedTicket.Title);
        Assert.Equal(TicketPriority.High, savedTicket.Priority);
    }
}
