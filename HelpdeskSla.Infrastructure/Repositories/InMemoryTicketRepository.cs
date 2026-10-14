using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Entities;

namespace HelpdeskSla.Infrastructure.Repositories;

public class InMemoryTicketRepository : ITicketRepository
{
    private readonly List<Ticket> _tickets = new();

    public void Add(Ticket ticket)
    {
        _tickets.Add(ticket);
    }

    public IReadOnlyList<Ticket> GetAll()
    {
        return _tickets.AsReadOnly();
    }
}