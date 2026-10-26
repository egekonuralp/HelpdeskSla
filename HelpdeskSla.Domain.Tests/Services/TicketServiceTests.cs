using HelpdeskSla.Api.Controllers;
using HelpdeskSla.Application.Common;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Domain.Tests.Fakes;
using HelpdeskSla.Infrastructure.Persistence;
using HelpdeskSla.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HelpdeskSla.Domain.Tests.Services;

public class TicketServiceTests
{
    private static void SetTicketId(Ticket ticket, int id)
    {
        var property = typeof(Ticket).GetProperty(nameof(Ticket.Id))!;

        property.SetValue(ticket, id);
    }

    [Fact]
    public void CreateTicket_ValidInputs_ShouldReturnOpenTicket()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var ticket = service.CreateTicket(
            "Sunucu çalışmıyor",
            "Sunucuya erişilemiyor",
            TicketPriority.High);

        // Assert
        Assert.Equal("Sunucu çalışmıyor", ticket.Title);
        Assert.Equal("Sunucuya erişilemiyor", ticket.Description);
        Assert.Equal(TicketPriority.High, ticket.Priority);
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public void AssignTicket_OpenTicket_ShouldAssignUser()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        var ticket = service.CreateTicket(
            "Sunucu çalışmıyor",
            "Sunucuya erişilemiyor",
            TicketPriority.High);

        // Act
        service.AssignTicket(ticket, 5);

        // Assert
        Assert.Equal(5, ticket.AssignedUserId);
        Assert.Equal(TicketStatus.Assigned, ticket.Status);
    }

    [Fact]
    public void CreateTicket_EmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange 
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            service.CreateTicket(
                "",
                "Sunucuya erişilemiyor",
                TicketPriority.Low));

        // Assert
        Assert.Equal("title", exception.ParamName); 
    }

    [Fact]
    public void AssignTicket_InvalidUserId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        var ticket = service.CreateTicket(
            "Sunucu çalışmıyor",
            "Sunucuya erişilemiyor",
            TicketPriority.High);

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => 
            service.AssignTicket(ticket, -1));

        // Assert
        Assert.Equal("userId", exception.ParamName);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Null(ticket.AssignedUserId);
    }

    [Fact]
    public void AssignTicket_AlreadyAssignedTicket_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        var ticket = service.CreateTicket(
            "Sunucu çalışmıyor",
            "Sunucuya erişilemiyor",
            TicketPriority.High);

        service.AssignTicket(ticket, 5);

        // Act
        Assert.Throws<InvalidOperationException>(() =>
            service.AssignTicket(ticket, 10));

        // Assert
        Assert.Equal(TicketStatus.Assigned, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);
    }

    [Fact]
    public void CreateTicket_ValidInputs_ShouldAddTicketToRepository()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var ticket = service.CreateTicket(
            "Sunucu çalışmıyor",
            "Sunucuya erişilemiyor",
            TicketPriority.High);

        // Assert
        Assert.Single(repository.Tickets);
        Assert.Same(ticket, repository.Tickets[0]);
    }

    [Fact]
    public void Constructor_NullRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new TicketService(null!, unitOfWork));

        // Assert
        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public void CreateTicket_RepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var repository = new ThrowingTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => 
            service.CreateTicket(
                "Sunucu çalışmıyor",
                "Sunucuya erişilemiyor",
                TicketPriority.High));

        // Assert
        Assert.Equal("Repository kayıt işlemi başarısız.", exception.Message);
    }

    [Fact]
    public void GetTicketById_NonExistingTicket_ReturnsNull()
    {
        // Arrange
        var repository = new InMemoryTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var tickets = service.GetTicketById(999);

        // Assert
        Assert.Null(tickets);
    }

    [Fact]
    public void GetAllTickets_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        var repository = new InMemoryTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var tickets = service.GetAllTickets();

        // Assert
        Assert.Empty(tickets);
    }

    [Fact]
    public void GetAllTickets_WithTwoTickets_ReturnsBothTickets()
    {
        // Arrange
        var repository = new InMemoryTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        var ticket1 = new Ticket(
            "Yazıcı Arızası",
            "Yazıcı çalışmıyor.",
            TicketPriority.Low);

        var ticket2 = new Ticket(
            "İnternet Sorunu",
            "İnternet bağlantısı kesiliyor.",
            TicketPriority.High);

        repository.Add(ticket1);
        repository.Add(ticket2);

        // Act
        var tickets = service.GetAllTickets();

        // Assert
        Assert.Equal(2, tickets.Count);
        Assert.Contains(tickets, t => t.Title == "Yazıcı Arızası");
        Assert.Contains(tickets, t => t.Title == "İnternet Sorunu");
    }

    [Fact]
    public async Task GetTicketByIdAsync_CanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var repository = new InMemoryTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => 
            service.GetTicketByIdAsync(1, cts.Token));
    }

    [Fact]
    public async Task CreateTicketAsync_ValidData_ReturnsCreatedTicket()
    {
        // Arrange
        var repository = new InMemoryTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var ticket = await service.CreateTicketAsync(
            "Async Ticket",
            "Asenkron oluşturma testi",
            TicketPriority.High);

        // Assert
        Assert.NotNull(ticket);
        Assert.Equal("Async Ticket", ticket.Title);
        Assert.Equal(TicketPriority.High, ticket.Priority);

        var savedTicket = Assert.Single(repository.GetAll());
        Assert.Same(ticket, savedTicket);

        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateTicketAsync_CanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var repository = new InMemoryTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.CreateTicketAsync(
                "Canceled Ticket",
                "İptal edilen kayıt testi",
                TicketPriority.Critical,
                cts.Token));

        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public async Task UpdateTicketTitleAsync_ExistingTicket_UpdatesDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);
        var unitOfWork = new EfUnitOfWork(context);

        var service = new TicketService(repository, unitOfWork);

        var ticket = new Ticket(
            "Eski Başlık",
            "Başlık güncelleme testi",
            TicketPriority.High);

        await repository.AddAsync(ticket);

        var expectedRowVersion = new byte[8]
        {
            1, 0, 0, 0, 0, 0, 0, 0
        };

        context.Entry(ticket)
            .Property(t => t.RowVersion)
            .CurrentValue = expectedRowVersion;
        await context.SaveChangesAsync();

        // Act
        var updatedTicket = await service.UpdateTicketTitleAsync(ticket.Id, "Yeni Başlık", expectedRowVersion);

        // Assert
        Assert.NotNull(updatedTicket);
        Assert.Equal("Yeni Başlık", updatedTicket.Title);

        using var newContext = new HelpdeskDbContext(options);

        var savedTicket = await newContext.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticket.Id);

        Assert.NotNull(savedTicket);
        Assert.Equal("Yeni Başlık", savedTicket.Title);
    }

    [Fact]
    public async Task UpdateTicketTitleAsync_NonExistingTicket_ThrowsKeyNotFoundException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);
        var unitOfWork = new EfUnitOfWork(context);

        var service = new TicketService(repository, unitOfWork);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateTicketTitleAsync(999, "Yeni Başlık", new byte[8]));
    }

    [Fact]
    public async Task UpdateTicketTitleAsync_EmptyTitle_ThrowsArgumentException()
    {
        // Arrange 
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HelpdeskDbContext(options);
        var repository = new EfTicketRepository(context);
        var unitOfWork = new EfUnitOfWork(context);

        var service = new TicketService(repository, unitOfWork);

        var ticket = new Ticket(
            "Eski Başlık",
            "Başlık güncelleme testi",
            TicketPriority.High);

        await repository.AddAsync(ticket);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateTicketTitleAsync(ticket.Id, "", new byte[8]));

        using var verificationContext = new HelpdeskDbContext(options);

        var savedTicket = await verificationContext.Tickets
            .AsNoTracking()
            .FirstAsync(t => t.Id == ticket.Id);

        Assert.Equal("Eski Başlık", savedTicket.Title);
    }

    [Fact]
    public void Constructor_NullUnitOfWork_ShouldThrowArgumentNullException()
    {
        // Arrange
        var repository = new InMemoryTicketRepository();

        // Act
        var exception =  Assert.Throws<ArgumentNullException>(() => 
            new TicketService(repository, null!));

        // Assert
        Assert.Equal("unitOfWork", exception.ParamName);
    }

    [Fact]
    public async Task GetTicketByIdAsync_InvalidId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            service.GetTicketByIdAsync(0));

        // Assert
        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public async Task GetTicketByIdAsync_TicketNotFound_ReturnsNull()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var result = await service.GetTicketByIdAsync(9999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTicketByIdAsync_ExistingTicket_ReturnsTicket()
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

        var ticketId = ticket.Id;

        // Act
        using var newContext = new HelpdeskDbContext(options);
        
        var repository = new EfTicketRepository(newContext);
        var unitOfWork = new EfUnitOfWork(newContext);

        var service = new TicketService(repository, unitOfWork);

        var result = await service.GetTicketByIdAsync(ticketId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticketId, result.Id);
        Assert.Equal("Yazıcı Arızası", result.Title);
        Assert.Equal(TicketPriority.Medium, result.Priority);
    }

    [Fact]
    public async Task GetAllTicketsAsync_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        // Act
        var result = await service.GetAllTicketsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllTicketsAsync_MultipleTickets_ReturnsAllTickets()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        var ticket1 = new Ticket(
            "Network Issue",
            "Network connection failed",
            TicketPriority.High);

        var ticket2 = new Ticket(
            "Printer Issue",
            "Printer is not responding",
            TicketPriority.Medium);

        var ticket3 = new Ticket(
            "Database Error",
            "Database connection failed",
            TicketPriority.Critical);

        repository.Add(ticket1);
        repository.Add(ticket2);
        repository.Add(ticket3);

        var result = await service.GetAllTicketsAsync();

        Assert.Equal(3, result.Count);
        Assert.Contains(ticket1, result);
        Assert.Contains(ticket2, result);
        Assert.Contains(ticket3, result);
        Assert.Equal(TicketPriority.High, ticket1.Priority);
        Assert.Equal(TicketPriority.Medium, ticket2.Priority);
        Assert.Equal(TicketPriority.Critical, ticket3.Priority);
    }

    [Fact]
    public async Task GetPagedTicketsAsync_EmptyRepository_ReturnsEmptyPage()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new TicketService(repository, unitOfWork);

        // Act
        var result = await service.GetPagedTicketsAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    [InlineData(1, 101)]
    public async Task GetPagedTicketsAsync_InvalidParameters_ThrowsArgumentOutOfRangeException(int pageNumber, int pageSize)
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.GetPagedTicketsAsync(pageNumber, pageSize));
    }

    [Fact]
    public async Task GetPagedTicketsAsync_StatusFilter_ReturnsOnlyOpenTickets()
    {
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var ticket1 = new Ticket(
            "Network Issue",
            "Network connection failed",
            TicketPriority.High);

        ticket1.Assign(5);
        ticket1.StartProgress();

        var ticket2 = new Ticket(
            "Printer Issue",
            "Printer is not responding",
            TicketPriority.Medium);

        var ticket3 = new Ticket(
            "Database Error",
            "Database connection failed",
            TicketPriority.Critical);

        repository.Tickets.Add(ticket1);
        repository.Tickets.Add(ticket2);
        repository.Tickets.Add(ticket3);
        await unitOfWork.SaveChangesAsync();

        var service = new TicketService(repository, unitOfWork);

        var parameters = new TicketQueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
            Status = TicketStatus.Open
        };

        var result = await service.GetPagedTicketsAsync(parameters);

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, ticket => Assert.Equal(TicketStatus.Open, ticket.Status));
    }

    [Fact]
    public async Task GetPagedTicketsAsync_InvalidStatus_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        // Act
        var parameters = new TicketQueryParameters
        {
            Status = (TicketStatus)999
        };

        // Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            service.GetPagedTicketsAsync(parameters));
    }

    [Fact]
    public async Task GetPagedTicketsAsync_InvalidPriority_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var parameters = new TicketQueryParameters
        {
            Priority = (TicketPriority)999
        };

        // Act
        var result = await controller.GetAll(parameters, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetPagedTicketsAsync_InvalidSortBy_ThrowsArgumentException()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        // Act
        var parameters = new TicketQueryParameters
        {
            SortBy = "banana"
        };

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetPagedTicketsAsync(parameters));
    }

    [Fact]
    public async Task GetPagedTicketsAsync_InvalidSortDirection_ThrowsArgumentException()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        // Act
        var parameters = new TicketQueryParameters
        {
            SortDirection = "fast"
        };

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetPagedTicketsAsync(parameters));
    }

    [Fact]
    public async Task AssignTicketAsync_ValidTicket_AssignsUserAndSavesChanges()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        var ticket = new Ticket(
            "Network connection problem",
            "User cannot connect to the network",
            TicketPriority.High);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var result = await service.AssignTicketAsync(ticket.Id, 5);

        // Assert
        Assert.True(result);
        Assert.Equal(TicketStatus.Assigned, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);
    }

    [Fact]
    public async Task AssignTicketAsync_TicketNotFound_ReturnsFalseWithoutSaving()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        // Act
        var result = await service.AssignTicketAsync(999, 5);

        // Assert
        Assert.False(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task AssignTicketAsync_InvalidUserId_ThrowsWithoutSaving()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        var ticket = new Ticket(
            "Network connection problem",
            "User cannot connect to the network",
            TicketPriority.High);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            service.AssignTicketAsync(ticket.Id, 0));

        // Assert
        Assert.Equal("userId", exception.ParamName);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Null(ticket.AssignedUserId);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task AssignTicketAsync_InProgressTicket_ThrowsWithoutSaving()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        var ticket = new Ticket(
            "Network connection problem",
            "User cannot connect to the network",
            TicketPriority.High);

        ticket.Assign(5);
        ticket.StartProgress();

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.AssignTicketAsync(ticket.Id, 10));

        // Assert
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}