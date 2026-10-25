using HelpdeskSla.Application.Common;
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

    public Ticket? GetById(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        return Tickets.FirstOrDefault(t => t.Id == id);
    }

    public IReadOnlyList<Ticket> GetAll()
    {
        return Tickets.AsReadOnly();
    }

    public Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GetById(id));
    }

    public Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GetAll());
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
        ArgumentNullException.ThrowIfNull(ticket);
        ArgumentNullException.ThrowIfNull(expectedRowVersion);

        if (expectedRowVersion.Length != 8)
        {
            throw new ArgumentException("RowVersion 8 byte uzunluğunda olmalıdır !!!", nameof(expectedRowVersion));
        }
    }

    public Task AddAuditLogAsync(TicketAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditLog);

        return Task.CompletedTask;
    }

    public Task<PagedResult<Ticket>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize <= 0 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var skip = ((long)pageNumber - 1) * pageSize;

        var items = skip > int.MaxValue
            ? new List<Ticket>()
            : Tickets
                .OrderBy(t => t.Id)
                .Skip((int)skip)
                .Take(pageSize)
                .ToList();

        var result = new PagedResult<Ticket>(items, pageNumber, pageSize, Tickets.Count);

        return Task.FromResult(result);
    }


    public Task<PagedResult<Ticket>> GetPagedAsync(TicketQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (parameters.PageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parameters.PageNumber));
        }

        if (parameters.PageSize <= 0 || parameters.PageSize > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(parameters.PageSize));
        }

        IEnumerable<Ticket> query = Tickets;

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

