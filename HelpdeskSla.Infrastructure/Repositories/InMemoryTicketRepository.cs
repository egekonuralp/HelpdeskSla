using HelpdeskSla.Application.Common;
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

    public Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var ticket = GetById(id);
        
        return Task.FromResult(ticket);
    }

    public Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tickets = GetAll();

        return Task.FromResult(tickets);
    }

    public Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Add(ticket);

        return Task.CompletedTask;
    }

    public Task<Ticket?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(GetById(id));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }

    public void SetOriginalRowVersion(Ticket ticket, byte[] expectedRowVersion)
    {
        throw new NotSupportedException("Bu repository RowVersion concurrency kontrolünü desteklemiyor.");
    }

    public Task AddAuditLogAsync(TicketAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("InMemoryTicketRepository audit log eklemeyi desteklemiyor.");
    }

    public Task<PagedResult<Ticket>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (pageNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));

        if (pageSize <= 0 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        var skip = ((long)pageNumber - 1) * pageSize;

        var orderedTickets = GetAll()
            .OrderBy(t => t.Id);

        var items = skip > int.MaxValue
            ? new List<Ticket>()
            : orderedTickets
                .Skip((int)skip)
                .Take(pageSize)
                .ToList();

        return Task.FromResult(new PagedResult<Ticket>(items, pageNumber, pageSize, GetAll().Count));
    }

    public Task<PagedResult<Ticket>> GetPagedAsync(TicketQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        cancellationToken.ThrowIfCancellationRequested();

        if (parameters.PageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parameters.PageNumber));
        }

        if (parameters.PageSize <= 0 || parameters.PageSize > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(parameters.PageSize));
        }

        var query = GetAll().AsEnumerable();

        if (parameters.Status.HasValue)
        {
            query = query.Where(t => t.Status == parameters.Status.Value);
        }

        var filteredTickets = query.OrderBy(t => t.Id).ToList();

        var skip = ((long)parameters.PageNumber - 1) * parameters.PageSize;

        var items = skip > int.MaxValue
            ? new List<Ticket>()
            : filteredTickets
                .Skip((int)skip)
                .Take(parameters.PageSize)
                .ToList();

        return Task.FromResult(new PagedResult<Ticket>(items, parameters.PageNumber, parameters.PageSize, filteredTickets.Count));
    }
}