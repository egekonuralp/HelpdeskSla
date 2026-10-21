using HelpdeskSla.Application.Exceptions;
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
        ArgumentNullException.ThrowIfNull(ticket);

        cancellationToken.ThrowIfCancellationRequested();

        await _context.Tickets.AddAsync(ticket, cancellationToken);
    }

    public async Task<Ticket?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new TicketConcurrencyException("Ticket başka bir kullanıcı tarafından güncellendi", ex);
        }
    }

    public void SetOriginalRowVersion(Ticket ticket, byte[] expectedRowVersion)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        ArgumentNullException.ThrowIfNull(expectedRowVersion);

        if (expectedRowVersion.Length != 8)
        {
            throw new ArgumentException("RowVersion 8 byte uzunluğunda olmalıdır !!!", nameof(expectedRowVersion));
        }

        _context.Entry(ticket)
            .Property(t => t.RowVersion)
            .OriginalValue = expectedRowVersion;
    }

    public async Task AddAuditLogAsync(TicketAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditLog);

        await _context.TicketAuditLogs.AddAsync(auditLog, cancellationToken);
    }
}