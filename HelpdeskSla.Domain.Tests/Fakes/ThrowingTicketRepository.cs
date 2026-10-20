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

    public void SetOriginalRowVersion(Ticket ticket, byte[] expectedRowVersion)
    {
        throw new NotSupportedException("Bu repository RowVersion concurrency kontrolünü desteklemiyor.");
    }

    public Task AddAuditLogAsync(TicketAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ThrowingTicketRepository audit log eklemeyi desteklemiyor.");
    }
}