namespace HelpdeskSla.Application.Interfaces;

public interface IUnitOfWork
{
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}