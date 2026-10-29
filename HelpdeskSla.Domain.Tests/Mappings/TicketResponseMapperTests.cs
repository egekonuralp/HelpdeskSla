using HelpdeskSla.Api.Mappings;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;

namespace HelpdeskSla.Domain.Tests.Mappings;

public class TicketResponseMapperTests
{
    [Fact]
    public void ToResponse_ValidTicket_MapsAllProperties()
    {
        // Arrange
        var ticket = new Ticket(
            "Network Issue",
            "Some network problems",
            TicketPriority.Critical);

        // Act
        var response = TicketResponseMapper.ToResponse(ticket);

        // Assert
        Assert.Equal(ticket.Id, response.Id);
        Assert.Equal(ticket.Title, response.Title);
        Assert.Equal(ticket.Description, response.Description);
        Assert.Equal(ticket.Status.ToString(), response.Status);
        Assert.Equal(ticket.Priority.ToString(), response.Priority);
        Assert.Equal(Convert.ToBase64String(ticket.RowVersion), response.RowVersion);
        Assert.Equal(ticket.CreatedAt, response.CreatedAt);
    }

    [Fact]
    public void ToResponse_NullTicket_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var exception = Assert.Throws<ArgumentNullException>(() => 
            TicketResponseMapper.ToResponse(null!));

        // Assert
        Assert.Equal("ticket", exception.ParamName);
    }
}

