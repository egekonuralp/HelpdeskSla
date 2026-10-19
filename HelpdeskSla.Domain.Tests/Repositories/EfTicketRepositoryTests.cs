using HelpdeskSla.Application.Exceptions;
using HelpdeskSla.Application.Services;
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

    [Fact]
    public async Task GetAllAsync_WithTwoTickets_ReturnsBothTickets()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repo = new EfTicketRepository(context);

        var ticket1 = new Ticket(
            "Yazıcı Arızası",
            "Yazıcı çalışmıyor.",
            TicketPriority.Low);

        var ticket2 = new Ticket(
            "İnternet Sorunu",
            "İnternet bağlantısı kesiliyor.",
            TicketPriority.High);

        repo.Add(ticket1);
        repo.Add(ticket2);

        // Act 
        var tickets = await repo.GetAllAsync();

        // Assert
        Assert.NotNull(tickets);
        Assert.Equal(2, tickets.Count);
        Assert.Contains(tickets, t => t.Title == "Yazıcı Arızası");
        Assert.Contains(tickets, t => t.Title == "İnternet Sorunu");
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repo = new EfTicketRepository(context);

        // Act
        var tickets = await repo.GetAllAsync();

        // Assert
        Assert.Empty(tickets);
    }

    [Fact]
    public async Task GetAllAsync_CanceledToken_ThrowsOperationCanceledException()
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
            repo.GetAllAsync(cts.Token));
    }

    [Fact]
    public async Task AddAsync_ValidTicket_SavesTicketToDatabase()
    {
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repo = new EfTicketRepository(context);

        var ticket = new Ticket(
            "Yazıcı Arızası",
            "Yazıcı çalışmıyor.",
            TicketPriority.Low);

        // Act
        await repo.AddAsync(ticket);

        // Assert
        var savedTicket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticket.Id);

        Assert.NotNull(savedTicket);
        Assert.Equal(ticket.Title, savedTicket.Title);
    }

    [Fact]
    public async Task AddAsync_NullTicket_ThrowsArgumentNullException()
    {
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repo = new EfTicketRepository(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            repo.AddAsync(null!));

        Assert.Equal("ticket", exception.ParamName);
    }

    [Fact]
    public async Task AddAsync_CanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange 
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repo = new EfTicketRepository(context);

        var ticket = new Ticket(
            "Yazıcı Arızası",
            "Yazıcı çalışmıyor.",
            TicketPriority.Low);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            repo.AddAsync(ticket, cts.Token));

        using var verificationContext = new HelpdeskDbContext(options);

        var savedTickets = await verificationContext.Tickets
            .AsNoTracking()
            .ToListAsync();

        Assert.Empty(savedTickets);
    }

    [Fact]
    public async Task Tracking_ChangedTicketTitle_SavesChangesToDatabase()
    {
        // Arrange 
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);

        var ticket = new Ticket(
            "Yazıcı Arızası",
            "Yazıcı çalışmıyor.",
            TicketPriority.Low);

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        // Act
        var trackedTicket = await context.Tickets
            .FirstAsync(t => t.Id == ticket.Id);

        trackedTicket.UpdateTitle("Yeni Başlık");
        await context.SaveChangesAsync();

        // Assert
        using var newContext = new HelpdeskDbContext(options);

        var savedTicket = await newContext.Tickets
            .AsNoTracking()
            .FirstAsync(t => t.Id == ticket.Id);

        Assert.Equal("Yeni Başlık", trackedTicket.Title);
    }

    [Fact]
    public async Task AsNoTracking_ChangedTicketTitle_DoesNotSaveChangesToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);

        var ticket = new Ticket(
            "Yazıcı Arızası",
            "Yazıcı çalışmıyor",
            TicketPriority.Medium);

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        using var newContext = new HelpdeskDbContext(options);

        var detachedTicket = await newContext.Tickets
            .AsNoTracking()
            .FirstAsync(t => t.Id == ticket.Id);

        // Act
        detachedTicket.UpdateTitle("Yeni Başlık");
        await newContext.SaveChangesAsync();

        // Assert
        using var extraContext = new HelpdeskDbContext(options);

        var savedTicket = await extraContext.Tickets
            .AsNoTracking()
            .FirstAsync(t => t.Id == ticket.Id);

        Assert.Equal("Yazıcı Arızası", savedTicket.Title);
    }

    [Fact]
    public async Task ChangeTracker_TracksEntityStatesCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);

        var ticket = new Ticket(
            "Yazıcı Arızası",
            "Yazıcı çalışmıyor",
            TicketPriority.Medium);

        // Act
        context.Tickets.Add(ticket);
        var state = context.Entry(ticket).State;

        await context.SaveChangesAsync();
        var stateAfterSave = context.Entry(ticket).State;

        ticket.UpdateTitle("Yeni Başlık");
        context.ChangeTracker.DetectChanges();
        var stateAfterUpdate = context.Entry(ticket).State;

        // Assert
        Assert.Equal(EntityState.Added, state);
        Assert.Equal(EntityState.Unchanged, stateAfterSave);
        Assert.Equal(EntityState.Modified, stateAfterUpdate);
    }

    [Fact]
    public async Task AsNoTracking_ReturnsDetachedEntity()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);

        var ticket = new Ticket(
            "Yazıcı Arızası",
            "Yazıcı çalışmıyor",
            TicketPriority.Medium);

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        using var newContext = new HelpdeskDbContext(options);

        var ticket2 = await newContext.Tickets
            .AsNoTracking()
            .FirstAsync(t => t.Id == ticket.Id);

        var state = newContext.Entry(ticket2).State;

        // Assert
        Assert.Equal(EntityState.Detached, state);
    }

    [Fact]
    public async Task ConcurrentUpdates_SecondSave_ThrowsConcurrencyException()
    {
        var databaseName = $"HelpdeskSlaConcurrencyTest_{Guid.NewGuid():N}";
        var connectionString = $@"Server=(localdb)\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        try
        {
            await using (var setupContext = new HelpdeskDbContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();

                var ticket = new Ticket(
                    "Eski Başlık",
                    "Concurrency Testi",
                    TicketPriority.High);

                setupContext.Tickets.Add(ticket);
                await setupContext.SaveChangesAsync();
            }

            await using var contextA = new HelpdeskDbContext(options);
            await using var contextB = new HelpdeskDbContext(options);

            var ticketA = await contextA.Tickets.SingleAsync();
            var ticketB = await contextB.Tickets.SingleAsync();

            ticketA.UpdateTitle("Personel A Güncelledi");
            await contextA.SaveChangesAsync();

            ticketB.UpdateTitle("Personel B Güncelledi");

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                contextB.SaveChangesAsync());

            await using var verificationContext = new HelpdeskDbContext(options);

            var savedTicket = await verificationContext.Tickets
                .AsNoTracking()
                .SingleAsync();

            Assert.Equal("Personel A Güncelledi", savedTicket.Title);
        }
        finally
        {
            await using var cleanupContext = new HelpdeskDbContext(options);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task SaveChangesAsync_ConcurrencyConflict_ThrowsTicketConcurrencyException()
    {
        // Arrange
        var databaseName = $"HelpdeskSlaConcurrencyTest_{Guid.NewGuid():N}";
        var connectionString = $@"Server=(localdb)\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        try
        {
            await using (var setupContext = new HelpdeskDbContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();

                var ticket = new Ticket(
                    "Eski Başlık",
                    "Concurrency Testi",
                    TicketPriority.High);

                setupContext.Tickets.Add(ticket);
                await setupContext.SaveChangesAsync();
            }

            await using var contextA = new HelpdeskDbContext(options);
            await using var contextB = new HelpdeskDbContext(options);

            var ticketA = await contextA.Tickets.SingleAsync();
            var ticketB = await contextB.Tickets.SingleAsync();

            ticketA.UpdateTitle("Personel A Güncelledi");
            await contextA.SaveChangesAsync();

            ticketB.UpdateTitle("Personel B Güncelledi");
            var repositoryB = new EfTicketRepository(contextB);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TicketConcurrencyException>(() =>
                repositoryB.SaveChangesAsync());

            Assert.IsType<DbUpdateConcurrencyException>(exception.InnerException);

            await using var verificationContext = new HelpdeskDbContext(options);

            var savedTicket = await verificationContext.Tickets
                .AsNoTracking()
                .SingleAsync();

            Assert.Equal("Personel A Güncelledi", savedTicket.Title);
        }
        finally
        {
            await using var cleanupContext = new HelpdeskDbContext(options);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task UpdateTicketTitleAsync_StaleRowVersion_ThrowsTicketConcurrencyException()
    {
        // Arrange
        var databaseName = $"HelpdeskSlaConcurrencyTest_{Guid.NewGuid():N}";
        var connectionString = $@"Server=(localdb)\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        int ticketId;

        try
        {
            await using (var setupContext = new HelpdeskDbContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();

                var ticket = new Ticket(
                    "Eski Başlık",
                    "Concurrency Testi",
                    TicketPriority.High);

                setupContext.Tickets.Add(ticket);
                await setupContext.SaveChangesAsync();

                ticketId = ticket.Id;
            }

            byte[] oldRowVersion;

            await using (var readContext = new HelpdeskDbContext(options))
            {
                var ticket = await readContext.Tickets.SingleAsync();

                oldRowVersion = ticket.RowVersion.ToArray();
            }

            await using (var otherContext = new HelpdeskDbContext(options))
            {
                var ticket = await otherContext.Tickets.SingleAsync();

                ticket.UpdateTitle("Personel B Güncelledi");

                await otherContext.SaveChangesAsync();
            }

            await using (var updateContext = new HelpdeskDbContext(options))
            {
                var repository = new EfTicketRepository(updateContext);
                var service = new TicketService(repository);

                await Assert.ThrowsAsync<TicketConcurrencyException>(() =>
                    service.UpdateTicketTitleAsync(ticketId, "Personel A Güncelledi", oldRowVersion));
            }

            await using (var verificationContext = new HelpdeskDbContext(options))
            {
                var savedTicket = await verificationContext.Tickets
                    .AsNoTracking()
                    .SingleAsync();

                Assert.Equal("Personel B Güncelledi", savedTicket.Title);
            }
        }
        finally
        {
            await using var cleanupContext = new HelpdeskDbContext(options);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }
}