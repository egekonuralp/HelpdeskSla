using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Entities;

namespace HelpdeskSla.Domain.Tests.Fakes;

public class FakeTicketRepository : ITicketRepository
{
    public List<Ticket> Tickets { get; } = new();

    public void Add(Ticket ticket)
    {
        Tickets.Add(ticket);
    }
}

