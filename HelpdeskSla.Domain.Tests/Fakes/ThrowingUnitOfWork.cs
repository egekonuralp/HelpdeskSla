using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Application.Exceptions;

namespace HelpdeskSla.Domain.Tests.Fakes;

public sealed class ThrowingUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Transactions are not supported by this test double.");
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;

        throw new TicketConcurrencyException("Ticket was modified by another user.");
    }
}
