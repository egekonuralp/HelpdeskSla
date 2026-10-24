using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Application.Common;

namespace HelpdeskSla.Application.Interfaces;

public interface ITicketRepository
{
    void Add(Ticket ticket);

    Ticket? GetById(int id);

    IReadOnlyList<Ticket> GetAll();

    Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);

    Task<Ticket?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken token = default);

    void SetOriginalRowVersion(Ticket ticket, byte[] expectedRowVersion);

    Task AddAuditLogAsync(TicketAuditLog auditLog, CancellationToken cancellationToken = default);

    Task<PagedResult<Ticket>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}