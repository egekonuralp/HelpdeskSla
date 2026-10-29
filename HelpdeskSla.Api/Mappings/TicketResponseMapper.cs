using HelpdeskSla.Api.DTOs;
using HelpdeskSla.Domain.Entities;

namespace HelpdeskSla.Api.Mappings;

public static class TicketResponseMapper
{
    public static TicketResponse ToResponse(Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        return new TicketResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority.ToString(),
            Status = ticket.Status.ToString(),
            CreatedAt = ticket.CreatedAt,
            RowVersion = Convert.ToBase64String(ticket.RowVersion)
        };
    }
}