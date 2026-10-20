using HelpdeskSla.Application.Exceptions;
using HelpdeskSla.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSla.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly HelpdeskDbContext _context;

    public EfUnitOfWork(HelpdeskDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        return new EfTransaction(transaction);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new TicketConcurrencyException("Ticket başka bir kullanıcı tarafından güncellendi", ex);
        }
    }
}