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
}