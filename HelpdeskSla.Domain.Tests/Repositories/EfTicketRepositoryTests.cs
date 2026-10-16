using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Infrastructure.Persistence;
using HelpdeskSla.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSla.Domain.Tests.Repositories;

public class EfTicketRepositoryTests
{
    [Fact]
    public void Add_ValidTicket_SavesTicketToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);

        var ticket = new Ticket(
            "Test Ticket",
            "Repository test açıklaması",
            TicketPriority.High);

        // Act
        repository.Add(ticket);

        // Assert       
        Assert.Equal(1, context.Tickets.Count());

        var savedTicket = context.Tickets.First();
        Assert.Equal("Test Ticket", savedTicket.Title);
    }

    [Fact]
    public void Add_NullTicket_ThrowsArgumentNullException()
    {
        // Arrange 
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            repository.Add(null!));

        // Assert
        Assert.Equal("ticket", exception.ParamName);
        Assert.Empty(context.Tickets);
    }

    [Fact]
    public void Add_ValidTicket_CanBeReadFromAnotherContext()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var ticket = new Ticket(
            "Persistent Ticket",
            "İkinci context üzerinden okunacak.",
            TicketPriority.Critical);

        // Act 
        using (var writeContext = new HelpdeskDbContext(options))
        {
            var repository = new EfTicketRepository(writeContext);

            repository.Add(ticket);
        }
        
        // Assert
        using (var readContext = new HelpdeskDbContext(options))
        {
            var savedTicket = readContext.Tickets
                .AsNoTracking()
                .FirstOrDefault();

            Assert.NotNull(savedTicket);
            Assert.Equal("Persistent Ticket", savedTicket.Title);
            Assert.Equal(TicketPriority.Critical, savedTicket.Priority);
        }
    }
}