namespace HelpdeskSla.Application.Interfaces;

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollBackAsync(CancellationToken cancellationToken = default);
}