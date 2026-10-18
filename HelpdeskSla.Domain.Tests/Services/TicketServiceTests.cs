using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Domain.Tests.Fakes;
using HelpdeskSla.Infrastructure.Repositories;
using Xunit;

namespace HelpdeskSla.Domain.Tests.Services;

public class TicketServiceTests
{
    [Fact]
    public void CreateTicket_ValidInputs_ShouldReturnOpenTicket()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var service = new TicketService(repository);

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
        var service = new TicketService(repository);

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
        var service = new TicketService(repository);

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
        var service = new TicketService(repository);

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
        var service = new TicketService(repository);

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
        var service = new TicketService(repository);

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
        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new TicketService(null!));

        // Assert
        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public void CreateTicket_RepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var repository = new ThrowingTicketRepository();
        var service = new TicketService(repository);

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
        var service = new TicketService(repository);

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
        var service = new TicketService(repository);

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
        var service = new TicketService(repository);

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
        var service = new TicketService(repository);

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
        var service = new TicketService(repository);

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
    }

    [Fact]
    public async Task CreateTicketAsync_CanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var repository = new InMemoryTicketRepository();
        var service = new TicketService(repository);

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
}