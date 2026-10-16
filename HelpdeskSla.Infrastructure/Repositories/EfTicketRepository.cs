using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Infrastructure.Persistence;

namespace HelpdeskSla.Infrastructure.Repositories;

public class EfTicketRepository : ITicketRepository
{
    private readonly HelpdeskDbContext _context;

    public EfTicketRepository(HelpdeskDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void Add(Ticket ticket)
    {
        if (ticket == null)
        {
            throw new ArgumentNullException(nameof(ticket));
        }

        _context.Tickets.Add(ticket);
        _context.SaveChanges();
    }
}