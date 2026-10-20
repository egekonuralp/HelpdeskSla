using HelpdeskSla.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace HelpdeskSla.Infrastructure.Persistence;

public sealed class EfTransaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return _transaction.CommitAsync(cancellationToken);
    }

    public Task RollBackAsync(CancellationToken cancellationToken = default)
    {
        return _transaction.RollbackAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _transaction.DisposeAsync();
    }
}