using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Enums;
using Xunit;

namespace HelpdeskSla.Domain.Tests.Services;

public class TicketServiceTests
{
    [Fact]
    public void CreateTicket_ValidInputs_ShouldReturnOpenTicket()
    {
        // Arrange
        var service = new TicketService();

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
        var service = new TicketService();

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
        var service = new TicketService();

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
        var service = new TicketService();

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
        var service = new TicketService();

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
}