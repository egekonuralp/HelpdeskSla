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

    public Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (id < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }
        
        Ticket? ticket = _tickets.FirstOrDefault(t => t.Id == id);

        return Task.FromResult(ticket);
    }

    public Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Ticket> tickets = _tickets.AsReadOnly();

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

        return GetByIdAsync(id, cancellationToken);
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

        IEnumerable<Ticket> query = _tickets;

        if (parameters.Status.HasValue)
        {
            query = query.Where(t => t.Status == parameters.Status.Value);
        }

        if (parameters.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == parameters.Priority.Value);
        }

        var totalCount = query.Count();

        var sortBy = parameters.SortBy?.ToLowerInvariant();
        var sortDirection = parameters.SortDirection?.ToLowerInvariant();

        var sortedQuery = (sortBy, sortDirection) switch
        {
            ("id", "asc") => query
                .OrderBy(t => t.Id),

            ("id", "desc") => query
                .OrderByDescending(t => t.Id),

            ("createdat", "asc") => query
                .OrderBy(t => t.CreatedAt)
                .ThenBy(t => t.Id),

            ("createdat", "desc") => query
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id),

            _ => throw new ArgumentException("Invalid sorting parameters.")
        };

        var skip = ((long)parameters.PageNumber - 1) * parameters.PageSize;

        var items = skip > int.MaxValue
            ? new List<Ticket>()
            : sortedQuery
                .Skip((int)skip)
                .Take(parameters.PageSize)
                .ToList();

        return Task.FromResult(new PagedResult<Ticket>(items, parameters.PageNumber, parameters.PageSize, totalCount));
    }
}