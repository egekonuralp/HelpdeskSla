using HelpdeskSla.Application.Interfaces;

namespace HelpdeskSla.Domain.Tests.Fakes;

public sealed class FakeUnitOfWork: IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken =  default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SaveChangesCallCount++;

        return Task.CompletedTask;
    }

    public Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Bu fake transaction testleri için kullanılmıyor.");
    }
}