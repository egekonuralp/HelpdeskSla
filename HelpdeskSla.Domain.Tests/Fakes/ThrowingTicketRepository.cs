using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Entities;

namespace HelpdeskSla.Domain.Tests.Fakes;

public class ThrowingTicketRepository : ITicketRepository
{
    public void Add(Ticket ticket)
    {
        throw new InvalidOperationException(
            "Repository kayıt işlemi başarısız.");
    }

    public Ticket? GetById(int id)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<Ticket> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Ticket?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}