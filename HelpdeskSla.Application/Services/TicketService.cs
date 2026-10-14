using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;

namespace HelpdeskSla.Application.Services;

public class TicketService
{
    public Ticket CreateTicket(string title, string description, TicketPriority priority)
    {
        return new Ticket(title, description, priority);
    }

    public void AssignTicket(Ticket ticket, int userId)
    {
        ticket.Assign(userId);
    }
}