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

    public Ticket? GetById(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        return _tickets.FirstOrDefault(t => t.Id == id);
    }

    public IReadOnlyList<Ticket> GetAll()
    {
        return _tickets.AsReadOnly();
    }
}