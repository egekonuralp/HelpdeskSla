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

    [Fact]
    public void GetById_ExistingTicket_ReturnsTicket()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);

        var ticket = new Ticket(
            "GetById Test",
            "Id üzerinden Ticket sorgulama",
            TicketPriority.High);

        repository.Add(ticket);

        // Act
        var savedTicket = repository.GetById(ticket.Id);

        // Assert
        Assert.NotNull(savedTicket);
        Assert.Equal("GetById Test", savedTicket.Title);
    }

    [Fact]
    public void GetById_NonExistingTicket_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);

        // Act
        var savedTicket = repository.GetById(999);

        // Assert
        Assert.Null(savedTicket);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetById_InvalidId_ThrowsArgumentOutOfRangeException(int id)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            repository.GetById(id));
    }

    [Fact]
    public void GetAll_WithTwoTickets_ReturnsBothTickets()
    {    
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);

        var ticket1 = new Ticket(
            "Ticket Title",
            "Ticket Description",
            TicketPriority.Critical);

        var ticket2 = new Ticket(
            "Ticket Title SS",
            "Ticket Description SS",
            TicketPriority.Critical);

        repository.Add(ticket1);
        repository.Add(ticket2);

        // Act
        var tickets = repository.GetAll();

        // Assert
        Assert.Equal(2, tickets.Count);
    }

    [Fact]
    public void GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);

        // Act
        var tickets = repository.GetAll();

        // Assert
        Assert.Empty(tickets);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTicket_ReturnsTicket()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repo = new EfTicketRepository(context);

        var ticket = new Ticket(
            "Async Test",
            "Asenkron Ticket sorgulama",
            TicketPriority.High);

        repo.Add(ticket);

        // Act
        var savedTicket = await repo.GetByIdAsync(ticket.Id);

        // Assert
        Assert.NotNull(savedTicket);
        Assert.Equal("Async Test", savedTicket.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingTicket_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repo = new EfTicketRepository(context);

        // Act
        var savedTicket = await repo.GetByIdAsync(999);

        // Assert
        Assert.Null(savedTicket);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_InvalidId_ThrowsArgumentOutOfRangeException(int id)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repo = new EfTicketRepository(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            repo.GetByIdAsync(id));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public async Task GetByIdAsync_CanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repo = new EfTicketRepository(context);

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        
        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => 
            repo.GetByIdAsync(1, cts.Token));
    }
}