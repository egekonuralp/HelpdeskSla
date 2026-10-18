using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public Ticket? GetById(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var savedTicket = _context.Tickets
            .AsNoTracking()
            .FirstOrDefault(t => t.Id == id);

        return savedTicket;
    }

    public IReadOnlyList<Ticket> GetAll()
    {
        var tickets = _context.Tickets
            .AsNoTracking()
            .ToList();

        return tickets;
    }

    public async Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        return await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);        
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        if (ticket == null)
        {
            throw new ArgumentNullException(nameof(ticket));
        }

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);
    }
}